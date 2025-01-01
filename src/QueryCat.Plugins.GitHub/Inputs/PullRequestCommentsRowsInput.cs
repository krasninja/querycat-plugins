using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub pull request comments input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/comments.
/// </remarks>
internal sealed class PullRequestCommentsRowsInput : BaseRowsInput<PullRequestReviewComment>
{
    [SafeFunction]
    [Description("Return GitHub comments for the specific pull request.")]
    [FunctionSignature("github_pull_comments(): object<IRowsInput>")]
    public static VariantValue PullRequestCommentsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new PullRequestCommentsRowsInput(thread));
    }

    public PullRequestCommentsRowsInput(IExecutionThread thread)
        : base(thread.ConfigStorage.GetOrDefault(General.GitHubToken))
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequestReviewComment> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "Comment id.")
            .AddProperty("pull_review_id", p => p.PullRequestReviewId, "Pull request review id.")
            .AddProperty("pull_number", DataType.Integer, _ => GetKeyColumnValue("pull_number"), "Pull request number.")
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("body", p => p.Body, "Comment body.")
            .AddProperty("url", p => p.HtmlUrl, "Comment URL.")
            .AddProperty("path", p => p.Path, "Relative path of the file the comment is about.")
            .AddProperty("commit_id", p => p.CommitId, "Commit id.")
            .AddProperty("original_commit_id", p => p.OriginalCommitId, "Original commit it.")
            .AddProperty("diff_hunk", p => p.DiffHunk, "Diff hunk the comment is about.")
            .AddProperty("in_reply_to_id", p => p.InReplyToId, "Id of the comment this comment replies to.")
            .AddProperty("position", p => p.Position, "Comment position.")
            .AddProperty("created_by_id", p => p.User.Id, "User id who created the comment.")
            .AddProperty("created_by_login", p => p.User.Login, "User login who created the comment.")
            .AddProperty("created_at", p => p.CreatedAt, "Date of comment creation.")
            .AddProperty("updated_at", p => p.UpdatedAt, "Date of comment update.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("pull_number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<PullRequestReviewComment> GetDataAsync(Fetcher<PullRequestReviewComment> fetcher,
        CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var pullNumber = GetKeyColumnValue("pull_number").ToInt32();
        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            return await Client.PullRequest.ReviewComment.GetAll(owner, repository, pullNumber,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        }, cancellationToken);
    }
}
