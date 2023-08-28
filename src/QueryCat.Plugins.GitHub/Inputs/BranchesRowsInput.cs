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
    [Description("Return Github branches of specific repository.")]
    [FunctionSignature("github_branches(): object<IRowsInput>")]
    public static VariantValue GitHubBranchesFunction(FunctionCallInfo args)
    {
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new BranchesRowsInput(token));
    }

    private string _owner = string.Empty;
    private string _repository = string.Empty;

    public BranchesRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Branch> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_branch.go.
        builder
            .AddProperty("repository_full_name", _ => GetFullRepositoryName(_owner, _repository), "The full name of the repository.")
            .AddProperty("name", p => p.Name, "Branch name.")
            .AddProperty("commit_sha", p => p.Commit.Sha, "Commit SHA the branch refers to.")
            .AddProperty("commit_url", p => p.Commit.Url, "Commit URL the branch refers to.")
            .AddProperty("protected", p => p.Protected, "True if branch is protected.");

        AddKeyColumn("repository_full_name",
            isRequired: true,
            set: v => (_owner, _repository) = SplitFullRepositoryName(v.AsString));
    }

    /// <inheritdoc />
    protected override IEnumerable<Branch> GetData(Fetcher<Branch> fetch)
    {
        fetch.PageStart = 1;
        return fetch.FetchPaged(async (page, limit, ct) =>
            {
                return await Client.Repository.Branch.GetAll(_owner, _repository,
                        new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            });
    }
}
