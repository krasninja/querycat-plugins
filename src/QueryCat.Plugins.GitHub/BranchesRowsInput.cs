using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Github;

/// <summary>
/// GitHub branches input.
/// </summary>
internal class BranchesRowsInput : BaseRowsInput<Branch>
{
    [Description("Return Github branches of specific repository.")]
    [FunctionSignature("github_branches(repository: string): object<IRowsInput>")]
    public static VariantValue GitHubBranchesFunction(FunctionCallInfo args)
    {
        var fullRepositoryName = args.GetAt(0).AsString;
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(GitHubFunctions.GitHubToken);

        return VariantValue.CreateFromObject(new BranchesRowsInput(fullRepositoryName, token));
    }

    public BranchesRowsInput(string fullRepositoryName, string token) : base(fullRepositoryName, token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Branch> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_branch.go.
        builder
            .AddProperty("name", p => p.Name, "Branch name.")
            .AddProperty("commit_sha", p => p.Commit.Sha, "Commit SHA the branch refers to.")
            .AddProperty("commit_url", p => p.Commit.Url, "Commit URL the branch refers to.")
            .AddProperty("protected", p => p.Protected, "True if branch is protected.");
    }

    /// <inheritdoc />
    protected override IEnumerable<Branch> GetData(ClassEnumerableInputFetch<Branch> fetch)
    {
        fetch.PageStart = 1;
        return fetch.FetchPaged(async (page, limit, ct) =>
            {
                return await Client.Repository.Branch.GetAll(Owner, Repository,
                        new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            });
    }
}
