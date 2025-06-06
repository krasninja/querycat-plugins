using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub pull request commits input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/pulls#list-commits-on-a-pull-request.
/// </remarks>
internal sealed class PullRequestCommitsRowsInput : BaseRowsInput<PullRequestCommit>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(PullRequestCommitsRowsInput));

    [SafeFunction]
    [Description("Return GitHub commits for the specific pull request.")]
    [FunctionSignature("github_pull_commits(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubPullRequestCommitsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new PullRequestCommitsRowsInput(token));
    }

    public PullRequestCommitsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequestCommit> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("pull_number", DataType.Integer, _ => GetKeyColumnValue("pull_number"), "Pull request number.")
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("node_id", p => p.NodeId, "Node id.")
            .AddProperty("message", p => p.Commit.Message, "Commit message.")
            .AddProperty("is_verified", p => p.Commit.Verification.Verified, "Is verified.")
            .AddProperty("sha", p => p.Sha, "Commit SHA.")
            .AddProperty("url", p => p.HtmlUrl, "Commit URL.")
            .AddProperty("committer_login", p => p.Committer?.Login, "Committer login.")
            .AddProperty("committer_email", p => p.Committer?.Email, "Committer email.")
            .AddProperty("author_login", p => p.Author?.Login, "Author login.")
            .AddProperty("author_email", p => p.Author?.Email, "Author email.")
            .AddProperty("author_date", p => p.Commit.Author?.Date, "Author contribution date.")
            .AddProperty("committer_date", p => p.Commit.Committer?.Date, "Committer contribution date.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("pull_number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<PullRequestCommit> GetDataAsync(Fetcher<PullRequestCommit> fetcher,
        CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var number = GetKeyColumnValue("pull_number").ToInt32();
        return fetcher.FetchAllAsync(
            async ct =>
            {
                _logger.LogDebug("Get for Repository = {Repository}, PullNumber = {Number}.", repository, number);
                var result = await Client.PullRequest.Commits(owner, repository, number);
                return result;
            }, cancellationToken);
    }
}
