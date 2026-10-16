using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub organizations input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/orgs/orgs.
/// </remarks>
internal sealed class OrganizationsRowsInput : BaseRowsInput<Organization>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(OrganizationsRowsInput));

    [SafeFunction]
    [Description("Return GitHub organizations.")]
    [FunctionSignature("github_organizations(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubOrganizationsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new OrganizationsRowsInput(token));
    }

    public OrganizationsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Organization> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_organization.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("login", p => p.Login, "The login name of the organization.")
            .AddProperty("id", p => p.Id, "The unique ID of the organization.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the organization.")
            .AddProperty("name", p => p.Name, "The name of the organization.")
            .AddProperty("company", p => p.Company, "The company name of the organization.")
            .AddProperty("description", p => p.Description, "The description of the organization.")
            .AddProperty("is_verified", p => p.IsVerified, "Whether the organization is verified.")
            .AddProperty("avatar_url", p => p.AvatarUrl, "The avatar URL of the organization.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the organization.")
            .AddProperty("blog", p => p.Blog, "The blog URL of the organization.")
            .AddProperty("email", p => p.Email, "The email address of the organization.")
            .AddProperty("location", p => p.Location, "The location of the organization.")
            .AddProperty("bio", p => p.Bio, "The bio of the organization.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the organization was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the organization was last updated.")
            .AddProperty("total_private_repos", p => p.TotalPrivateRepos, "The total number of private repositories.")
            .AddProperty("owned_private_repos", p => p.OwnedPrivateRepos, "The number of owned private repositories.")
            .AddProperty("public_repos", p => p.PublicRepos, "The number of public repositories.")
            .AddProperty("public_gists", p => p.PublicGists, "The number of public gists.")
            .AddProperty("private_gists", p => p.PrivateGists, "The number of private gists.")
            .AddProperty("followers", p => p.Followers, "The number of followers.")
            .AddProperty("following", p => p.Following, "The number of following.")
            .AddProperty("collaborators", p => p.Collaborators, "The number of collaborators.")
            .AddProperty("disk_usage", p => p.DiskUsage, "The disk usage in kilobytes.")
            .AddProperty("type", p => p.Type, "The type of the account.")
            .AddProperty("url", p => p.Url, "The API URL of the organization.")
            .AddKeyColumn("login", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Organization> GetDataAsync(Fetcher<Organization> fetcher,
        CancellationToken cancellationToken = default)
    {
        var login = GetKeyColumnValue("login");

        if (!login.IsNull)
        {
            return fetcher.FetchOneAsync(
                async ct =>
                {
                    _logger.LogDebug("Get organization by login = {Login}.", login);
                    return await Client.Organization.Get(login.AsString);
                }, cancellationToken);
        }
        else
        {
            fetcher.PageStart = 1;
            return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            {
                _logger.LogDebug("Get organizations for the authenticated user.");
                return await Client.Organization.GetAllForCurrent(
                    new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            }, cancellationToken);
        }
    }
}
