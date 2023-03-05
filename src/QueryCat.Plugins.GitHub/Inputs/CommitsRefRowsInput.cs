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
internal class CommitsRefRowsInput : CommitsRowsInput
{
    [Description("Return Github commit info of specific repository.")]
    [FunctionSignature("github_commits_ref(): object<IRowsInput>")]
    public static VariantValue GitHubCommitsRefFunction(FunctionCallInfo args)
    {
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new CommitsRefRowsInput(token));
    }

    private string _sha = string.Empty;
    private string _owner = string.Empty;
    private string _repository = string.Empty;

    public CommitsRefRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitHubCommit> builder)
    {
        base.Initialize(builder);
        builder
            .AddProperty("repository_full_name", _ => GetFullRepositoryName(_owner, _repository), "The full name of the repository.")
            .AddProperty("sha", _ => _sha, "SHA of the commit.")
            .AddProperty("additions", p => p.Stats.Additions, "The number of additions in the commit.")
            .AddProperty("deletions", p => p.Stats.Deletions, "The number of deletions in the commit.");
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        inputInfo
            .AddKeyColumn("repository_full_name",
                isRequired: true,
                action: v => (_owner, _repository) = SplitFullRepositoryName(v.AsString))
            .AddKeyColumn("sha",
                isRequired: true,
                action: v => _sha = v.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<GitHubCommit> GetData(ClassEnumerableInputFetch<GitHubCommit> fetch)
    {
        return fetch.FetchOne(async ct => await Client.Repository.Commit.Get(_owner, _repository, _sha));
    }
}
