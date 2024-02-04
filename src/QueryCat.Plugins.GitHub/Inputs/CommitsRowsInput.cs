using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
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
    [Description("Return Github commits of specific repository.")]
    [FunctionSignature("github_commits(): object<IRowsInput>")]
    public static VariantValue GitHubCommitsFunction(FunctionCallInfo args)
    {
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new CommitsRowsInput(token));
    }

    private readonly CommitRequest _request = new();
    private string _owner = string.Empty;
    private string _repository = string.Empty;

    public CommitsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitHubCommit> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_commit.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", _ => GetFullRepositoryName(_owner, _repository), "The full name of the repository.")
            .AddProperty("sha", p => p.Sha, "SHA of the commit.")
            .AddProperty("author_name", p => p.Commit.Author.Name, "The login name of the author of the commit.")
            .AddProperty("author_date", p => p.Commit.Author.Date, "Timestamp when the author made this commit.")
            .AddProperty("committer_login", p => p.Committer.Login, "The login name of committer of the commit.")
            .AddProperty("verified", p => p.Commit.Verification.Verified, "True if the commit was verified with a signature.")
            .AddProperty("message", p => p.Commit.Message, "Commit message.");

        AddKeyColumn("repository_full_name",
            isRequired: true,
            set: v => (_owner, _repository) = SplitFullRepositoryName(v.AsString));
        AddKeyColumn("sha",
            operation: VariantValue.Operation.GreaterOrEquals, VariantValue.Operation.Greater,
            set: v => _request.Sha = v.AsString);
        AddKeyColumn("author_date",
            operation: VariantValue.Operation.Greater,
            orOperation: VariantValue.Operation.GreaterOrEquals,
            set: v => _request.Since = v.AsTimestamp);
        AddKeyColumn("author_date",
            operation: VariantValue.Operation.Less,
            orOperation: VariantValue.Operation.LessOrEquals,
            set: v => _request.Until = v.AsTimestamp);
        AddKeyColumn("author_login",
            operation: VariantValue.Operation.Equals,
            set: v => _request.Author = v.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<GitHubCommit> GetData(Fetcher<GitHubCommit> fetcher)
    {
        fetcher.PageStart = 1;
        return fetcher.FetchPaged(async (page, limit, ct) =>
        {
            return await Client.Repository.Commit.GetAll(
                _owner,
                _repository,
                _request,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        });
    }
}
