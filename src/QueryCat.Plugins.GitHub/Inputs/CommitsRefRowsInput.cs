using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub commit SHA input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/commits/commits#get-a-commit.
/// </remarks>
internal sealed class CommitsRefRowsInput : CommitsRowsInput
{
    [SafeFunction]
    [Description("Return Github commit info of specific repository.")]
    [FunctionSignature("github_commits_ref(): object<IRowsInput>")]
    public static VariantValue GitHubCommitsRefFunction(FunctionCallInfo args)
    {
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new CommitsRefRowsInput(token));
    }

    public CommitsRefRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitHubCommit> builder)
    {
        base.Initialize(builder);
        builder
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("sha", _ => GetKeyColumnValue("sha"), "SHA of the commit.")
            .AddProperty("additions", p => p.Stats.Additions, "The number of additions in the commit.")
            .AddProperty("deletions", p => p.Stats.Deletions, "The number of deletions in the commit.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("sha", isRequired: true);
    }

    /// <inheritdoc />
    protected override IEnumerable<GitHubCommit> GetData(Fetcher<GitHubCommit> fetch)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var sha = GetKeyColumnValue("sha").AsString;
        return fetch.FetchOne(async ct => await Client.Repository.Commit.Get(owner, repository, sha));
    }
}
