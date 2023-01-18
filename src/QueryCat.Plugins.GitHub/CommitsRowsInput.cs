using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Github;

/// <summary>
/// GitHub commits input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/commits/commits.
/// </remarks>
internal class CommitsRowsInput : BaseRowsInput<GitHubCommit>
{
    [Description("Return Github commits of specific repository.")]
    [FunctionSignature("github_commits(repository: string): object<IRowsInput>")]
    public static VariantValue GitHubCommitsFunction(FunctionCallInfo args)
    {
        var fullRepositoryName = args.GetAt(0).AsString;
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(GitHubFunctions.GitHubToken);

        return VariantValue.CreateFromObject(new CommitsRowsInput(fullRepositoryName, token));
    }

    private readonly CommitRequest _request = new();

    public CommitsRowsInput(string fullRepositoryName, string token) : base(fullRepositoryName, token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitHubCommit> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_commit.go.
        builder
            .AddDataProperty()
            .AddProperty("sha", p => p.Sha, "SHA of the commit.")
            .AddProperty("author_name", p => p.Commit.Author.Name, "The login name of the author of the commit.")
            .AddProperty("author_date", p => p.Commit.Author.Date, "Timestamp when the author made this commit.")
            .AddProperty("committer_login", p => p.Committer.Login, "The login name of committer of the commit.")
            .AddProperty("verified", p => p.Commit.Verification.Verified, "True if the commit was verified with a signature.")
            .AddProperty("message", p => p.Commit.Message, "Commit message.");
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        inputInfo
            .SetInputArguments(Owner, Repository)
            .AddKeyColumn("sha",
                operation: VariantValue.Operation.GreaterOrEquals, VariantValue.Operation.Greater,
                action: v => _request.Sha = v.AsString)
            .AddKeyColumn("author_date",
                operation: VariantValue.Operation.Greater,
                orOperation: VariantValue.Operation.GreaterOrEquals,
                action: v => _request.Since = v.AsTimestamp)
            .AddKeyColumn("author_date",
                operation: VariantValue.Operation.Less,
                orOperation: VariantValue.Operation.LessOrEquals,
                action: v => _request.Until = v.AsTimestamp)
            .AddKeyColumn("author_login",
                operation: VariantValue.Operation.Equals,
                action: v => _request.Author = v.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<GitHubCommit> GetData(ClassEnumerableInputFetch<GitHubCommit> fetch)
    {
        fetch.PageStart = 1;
        return fetch.FetchPaged(async (page, limit, ct) =>
        {
            return await Client.Repository.Commit.GetAll(
                Owner,
                Repository,
                _request,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        });
    }
}
