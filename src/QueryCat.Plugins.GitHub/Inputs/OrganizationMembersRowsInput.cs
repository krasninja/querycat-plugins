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
/// GitHub organization members input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/orgs/members.
/// </remarks>
internal sealed class OrganizationMembersRowsInput : BaseRowsInput<User>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(OrganizationMembersRowsInput));

    [SafeFunction]
    [Description("Return GitHub organization members.")]
    [FunctionSignature("github_organization_members(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubOrganizationMembersFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new OrganizationMembersRowsInput(token));
    }

    public OrganizationMembersRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<User> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_organization_member.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("organization", DataType.String, _ => GetKeyColumnValue("organization"), "The organization login name.")
            .AddProperty("login", p => p.Login, "The login name of the member.")
            .AddProperty("id", p => p.Id, "The unique ID of the member.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the member.")
            .AddProperty("name", p => p.Name, "The name of the member.")
            .AddProperty("company", p => p.Company, "The company of the member.")
            .AddProperty("blog", p => p.Blog, "The blog URL of the member.")
            .AddProperty("location", p => p.Location, "The location of the member.")
            .AddProperty("email", p => p.Email, "The email address of the member.")
            .AddProperty("bio", p => p.Bio, "The bio of the member.")
            .AddProperty("avatar_url", p => p.AvatarUrl, "The avatar URL of the member.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the member's profile.")
            .AddProperty("type", p => p.Type, "The type of account.")
            .AddProperty("site_admin", p => p.SiteAdmin, "Whether the member is a site administrator.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the member was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the member was last updated.")
            .AddProperty("followers", p => p.Followers, "The number of followers.")
            .AddProperty("following", p => p.Following, "The number of following.")
            .AddProperty("public_repos", p => p.PublicRepos, "The number of public repositories.")
            .AddProperty("public_gists", p => p.PublicGists, "The number of public gists.")
            .AddKeyColumn("organization", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<User> GetDataAsync(Fetcher<User> fetcher,
        CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization");

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            _logger.LogDebug("Get members for organization = {Organization}.", organization);
            return await Client.Organization.Member.GetAll(organization.AsString,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        }, cancellationToken);
    }
}
