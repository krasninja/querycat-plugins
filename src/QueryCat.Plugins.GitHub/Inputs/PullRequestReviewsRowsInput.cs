using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// List reviews for a pull request.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/reviews.
/// </remarks>
internal sealed class PullRequestReviewsRowsInput : BaseRowsInput<PullRequestReview>
{
    [SafeFunction]
    [Description("Return GitHub reviews for the specific pull request.")]
    [FunctionSignature("github_pull_reviews(): object<IRowsInput>")]
    public static VariantValue PullReviewsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new PullRequestReviewsRowsInput(thread));
    }

    public PullRequestReviewsRowsInput(IExecutionThread thread)
        : base(thread.ConfigStorage.GetOrDefault(General.GitHubToken))
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequestReview> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_pull_request_review.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("id", p => p.Id, "The identifier of the review.")
            .AddProperty("body", p => p.Body, "The body of review.")
            .AddProperty("state", p => p.State.StringValue, "The state (approve, comment, etc).")
            .AddProperty("url", p => p.PullRequestUrl, "The review URL.")
            .AddProperty("submitted_at", p => p.SubmittedAt, "Identifies when the Pull Request Review was submitted.")
            .AddProperty("author_login", p => p.User.Login, "Author login.")
            .AddProperty("author_email", p => p.User.Email, "Author email.")
            .AddProperty("author_association", p => p.AuthorAssociation.StringValue,
                "Author's association with the subject of the PR the review was raised on.")
            .AddProperty("commit_id", p => p.CommitId, "Commit identifier.")
            .AddProperty("pull_number", DataType.Integer, p => GetKeyColumnValue("pull_number"), "Pull request number.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("pull_number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<PullRequestReview> GetDataAsync(Fetcher<PullRequestReview> fetcher,
        CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var number = GetKeyColumnValue("pull_number").ToInt32();
        return fetcher.FetchAllAsync(
            async ct => await Client.Repository.PullRequest.Review.GetAll(owner, repository, number), cancellationToken);
    }
}
