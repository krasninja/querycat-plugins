using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repositories input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/repos/repos.
/// </remarks>
internal sealed class RepositoriesRowsInput : BaseRowsInput<Repository>
{
    [SafeFunction]
    [Description("Return GitHub repositories.")]
    [FunctionSignature("github_repositories(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubRepositoriesFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new RepositoriesRowsInput(token));
    }

    public RepositoriesRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Repository> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_repository.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "The unique ID of the repository.")
            .AddProperty("node_id", p => p.NodeId, "The GraphQL node ID of the repository.")
            .AddProperty("name", p => p.Name, "The name of the repository.")
            .AddProperty("full_name", p => p.FullName, "The full name of the repository (owner/repo).")
            .AddProperty("owner_login", p => p.Owner.Login, "The login name of the repository owner.")
            .AddProperty("owner_id", p => p.Owner.Id, "The unique ID of the repository owner.")
            .AddProperty("owner_type", p => p.Owner.Type, "The type of the repository owner (User or Organization).")
            .AddProperty("description", p => p.Description, "The description of the repository.")
            .AddProperty("homepage", p => p.Homepage, "The homepage URL of the repository.")
            .AddProperty("language", p => p.Language, "The primary language of the repository.")
            .AddProperty("is_private", p => p.Private, "Whether the repository is private.")
            .AddProperty("is_fork", p => p.Fork, "Whether the repository is a fork.")
            .AddProperty("is_template", p => p.IsTemplate, "Whether the repository is a template repository.")
            .AddProperty("is_archived", p => p.Archived, "Whether the repository is archived.")
            .AddProperty("visibility", p => p.Visibility?.ToString(), "The visibility of the repository (public, private, or internal).")
            .AddProperty("default_branch", p => p.DefaultBranch, "The default branch of the repository.")
            .AddProperty("stargazers_count", p => p.StargazersCount, "The number of stars on the repository.")
            .AddProperty("forks_count", p => p.ForksCount, "The number of forks of the repository.")
            .AddProperty("open_issues_count", p => p.OpenIssuesCount, "The number of open issues in the repository.")
            .AddProperty("subscribers_count", p => p.SubscribersCount, "The number of subscribers to the repository.")
            .AddProperty("size", p => p.Size, "The size of the repository in kilobytes.")
            .AddProperty("has_issues", p => p.HasIssues, "Whether the repository has issues enabled.")
            .AddProperty("has_wiki", p => p.HasWiki, "Whether the repository has wiki enabled.")
            .AddProperty("has_pages", p => p.HasPages, "Whether the repository has GitHub Pages enabled.")
            .AddProperty("has_downloads", p => p.HasDownloads, "Whether the repository has downloads enabled.")
            .AddProperty("has_discussions", p => p.HasDiscussions, "Whether the repository has discussions enabled.")
            .AddProperty("url", p => p.Url, "The API URL of the repository.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the repository.")
            .AddProperty("clone_url", p => p.CloneUrl, "The clone URL of the repository.")
            .AddProperty("git_url", p => p.GitUrl, "The git URL of the repository.")
            .AddProperty("ssh_url", p => p.SshUrl, "The SSH URL of the repository.")
            .AddProperty("svn_url", p => p.SvnUrl, "The SVN URL of the repository.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the repository was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the repository was last updated.")
            .AddProperty("pushed_at", p => p.PushedAt, "The timestamp when the repository was last pushed to.")
            .AddProperty("license_name", p => p.License?.Name, "The name of the license.")
            .AddProperty("license_key", p => p.License?.Key, "The key of the license.")
            .AddKeyColumn("organization", isRequired: false)
            .AddKeyColumn("full_name", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Repository> GetDataAsync(Fetcher<Repository> fetcher,
        CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization");
        var fullName = GetKeyColumnValue("full_name");

        if (!fullName.IsNull)
        {
            var (owner, repo) = SplitFullRepositoryName(fullName.AsString);
            return fetcher.FetchOneAsync(
                async ct =>
                {
                    return await Client.Repository.Get(owner, repo);
                }, cancellationToken);
        }
        else if (!organization.IsNull)
        {
            fetcher.PageStart = 1;
            return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            {
                return await Client.Repository.GetAllForOrg(organization.AsString,
                    new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            }, cancellationToken);
        }
        else
        {
            fetcher.PageStart = 1;
            return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            {
                return await Client.Repository.GetAllForCurrent(
                    new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            }, cancellationToken);
        }
    }
}
