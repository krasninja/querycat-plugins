using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub branches input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/branches/branches.
/// </remarks>
internal sealed class BranchesRowsInput : BaseRowsInput<Branch>
{
    [SafeFunction]
    [Description("Return Github branches of specific repository.")]
    [FunctionSignature("github_branches(): object<IRowsInput>")]
    public static VariantValue GitHubBranchesFunction(FunctionCallInfo args)
    {
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new BranchesRowsInput(token));
    }

    public BranchesRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Branch> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_branch.go.
        builder
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("name", p => p.Name, "Branch name.")
            .AddProperty("commit_sha", p => p.Commit.Sha, "Commit SHA the branch refers to.")
            .AddProperty("commit_url", p => p.Commit.Url, "Commit URL the branch refers to.")
            .AddProperty("protected", p => p.Protected, "True if branch is protected.")
            .AddKeyColumn("repository_full_name", isRequired: true);
    }

    /// <inheritdoc />
    protected override IEnumerable<Branch> GetData(Fetcher<Branch> fetch)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));

        fetch.PageStart = 1;
        return fetch.FetchPaged(async (page, limit, ct) =>
            {
                return await Client.Repository.Branch.GetAll(owner, repository,
                        new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            });
    }
}
