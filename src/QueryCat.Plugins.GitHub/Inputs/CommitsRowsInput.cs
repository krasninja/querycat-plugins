using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub commits input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/commits/commits.
/// </remarks>
internal class CommitsRowsInput : BaseRowsInput<GitHubCommit>
{
    [SafeFunction]
    [Description("Return GitHub commits of specific repository.")]
    [FunctionSignature("github_commits(): object<IRowsInput>")]
    public static VariantValue GitHubCommitsFunction(IExecutionThread thread)
    {
        var token = thread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new CommitsRowsInput(token));
    }

    public CommitsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitHubCommit> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_commit.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("sha", p => p.Sha, "SHA of the commit.")
            .AddProperty("author_name", p => p.Commit.Author.Name, "The login name of the author of the commit.")
            .AddProperty("author_login", p => p.Commit.Author.Email, "The email name of the author of the commit.")
            .AddProperty("author_date", p => p.Commit.Author.Date, "Timestamp when the author made this commit.")
            .AddProperty("committer_login", p => p.Committer.Login, "The login name of committer of the commit.")
            .AddProperty("verified", p => p.Commit.Verification.Verified, "True if the commit was verified with a signature.")
            .AddProperty("message", p => p.Commit.Message, "Commit message.")
                .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("sha", operation: VariantValue.Operation.GreaterOrEquals, VariantValue.Operation.Greater)
            .AddKeyColumn("author_date",
                operation: VariantValue.Operation.Greater,
                orOperation: VariantValue.Operation.GreaterOrEquals)
            .AddKeyColumn("author_date",
                operation: VariantValue.Operation.Less,
                orOperation: VariantValue.Operation.LessOrEquals)
            .AddKeyColumn("author_name", operation: VariantValue.Operation.Equals);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<GitHubCommit> GetDataAsync(Fetcher<GitHubCommit> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var request = new CommitRequest();
        if (TryGetKeyColumnValue("committer_login", VariantValue.Operation.Equals, out var value))
        {
            request.Author = value.AsString;
        }
        if (TryGetKeyColumnValue("author_date", VariantValue.Operation.Greater, out value))
        {
            request.Since = value.AsTimestamp;
        }
        if (TryGetKeyColumnValue("author_date", VariantValue.Operation.Less, out value))
        {
            request.Until = value.AsTimestamp;
        }
        if (TryGetKeyColumnValue("sha", VariantValue.Operation.Equals, out value))
        {
            request.Sha = value.AsString;
        }
        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            return await Client.Repository.Commit.GetAll(
                owner,
                repository,
                request,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        }, cancellationToken);
    }
}
