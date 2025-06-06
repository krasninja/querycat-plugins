using System.ComponentModel;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// List reviews for a pull request "request reviews".
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/review-requests.
/// </remarks>
internal sealed class PullRequestRequestedReviewsRowsInput : BaseRowsInput<PullRequestRequestedReviewsRowsInput.ReviewRequest>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(PullRequestRequestedReviewsRowsInput));

    internal sealed class ReviewRequest
    {
        public required string Type { get; init; }

        public required long Id { get; init; }

        public required string Name { get; init; }

        public required string LdapName { get; init; }

        public required string UserLogin { get; init; }

        public required string HtmlUrl { get; init; }
    }

    [SafeFunction]
    [Description("Return GitHub review requests for the specific pull request.")]
    [FunctionSignature("github_pull_reviews_requests(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubPullRequestedReviewsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new PullRequestRequestedReviewsRowsInput(token));
    }

    public PullRequestRequestedReviewsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<ReviewRequest> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_pull_request_review.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("type", p => p.Type, "Type (team or user).")
            .AddProperty("id", p => p.Id, "The identifier of the review request.")
            .AddProperty("name", p => p.Name, "Name of user or team.")
            .AddProperty("ldap_name", p => p.LdapName, "LDAP distinguished name.")
            .AddProperty("user_login", p => p.UserLogin, "Login for user type items.")
            .AddProperty("html_url", p => p.HtmlUrl, "HTML URL.")
            .AddProperty("pull_number", DataType.Integer, p => GetKeyColumnValue("pull_number"), "Pull request number.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("pull_number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<ReviewRequest> GetDataAsync(Fetcher<ReviewRequest> fetcher,
        CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var number = GetKeyColumnValue("pull_number").ToInt32();
        return fetcher.FetchAllAsync(async ct =>
        {
            _logger.LogDebug("Get for Repository = {Repository}, PullNumber = {Number}.", repository, number);
            var result = await Client.Repository.PullRequest.ReviewRequest.Get(owner, repository, number);
            return Array.Empty<ReviewRequest>()
                .Union(result.Teams.Select(t => new ReviewRequest
                {
                    Type = "team",
                    Id = t.Id,
                    Name = t.Name,
                    LdapName = t.LdapDistinguishedName,
                    UserLogin = string.Empty,
                    HtmlUrl = t.HtmlUrl,
                }))
                .Union(result.Users.Select(u => new ReviewRequest
                {
                    Type = "user",
                    Id = u.Id,
                    Name = u.Name,
                    LdapName = u.LdapDistinguishedName,
                    UserLogin = u.Login,
                    HtmlUrl = u.HtmlUrl,
                }));
        }, cancellationToken);
    }
}
