using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub commit SHA input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/commits/commits#get-a-commit.
/// </remarks>
internal class CommitsRowsRefInput : CommitsRowsInput
{
    private readonly string _sha;

    [Description("Return Github commit info of specific repository.")]
    [FunctionSignature("github_commits_ref(repository: string, sha: string): object<IRowsInput>")]
    public static VariantValue GitHubCommitsRefFunction(FunctionCallInfo args)
    {
        var fullRepositoryName = args.GetAt(0).AsString;
        var sha = args.GetAt(1).AsString;
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(Functions.GitHubToken);

        return VariantValue.CreateFromObject(new CommitsRowsRefInput(fullRepositoryName, sha, token));
    }

    public CommitsRowsRefInput(string fullRepositoryName, string sha, string token) : base(fullRepositoryName, token)
    {
        _sha = sha;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitHubCommit> builder)
    {
        base.Initialize(builder);
        builder
            .AddProperty("additions", p => p.Stats.Additions, "The number of additions in the commit.")
            .AddProperty("deletions", p => p.Stats.Deletions, "The number of deletions in the commit.");
    }

    /// <inheritdoc />
    protected override IEnumerable<GitHubCommit> GetData(ClassEnumerableInputFetch<GitHubCommit> fetch)
    {
        return fetch.FetchOne(async ct => await Client.Repository.Commit.Get(Owner, Repository, _sha));
    }
}
