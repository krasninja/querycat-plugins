using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub team members input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/teams/members#list-team-members.
/// </remarks>
internal sealed class TeamMembersRowsInput : BaseRowsInput<User>
{
    [SafeFunction]
    [Description("Return GitHub members of a specific team.")]
    [FunctionSignature("github_team_members(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubTeamMembersFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new TeamMembersRowsInput(token));
    }

    public TeamMembersRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<User> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_team_member.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("organization", DataType.String, _ => GetKeyColumnValue("organization"), "The organization login name.")
            .AddProperty("team_slug", DataType.String, _ => GetKeyColumnValue("team_slug"), "The team slug name.")
            .AddProperty("login", p => p.Login, "The login name of the team member.")
            .AddProperty("id", p => p.Id, "The unique ID of the team member.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the team member.")
            .AddProperty("avatar_url", p => p.AvatarUrl, "The avatar URL of the team member.")
            .AddProperty("html_url", p => p.HtmlUrl, "The GitHub profile URL of the team member.")
            .AddProperty("type", p => p.Type, "The type of the team member (User or Bot).")
            .AddProperty("site_admin", p => p.SiteAdmin, "True if the team member is a site admin.")
            .AddKeyColumn("organization", isRequired: true)
            .AddKeyColumn("team_slug", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<User> GetDataAsync(Fetcher<User> fetcher, CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization").AsString;
        var teamSlug = GetKeyColumnValue("team_slug").AsString;

        return fetcher.FetchAllAsync(async ct =>
        {
            // First get the team by slug to get the team ID.
            var team = await Client.Organization.Team.GetByName(organization, teamSlug);

            // Then get all members of that team.
            var members = await Client.Organization.Team.GetAllMembers(team.Id);
            return members;
        }, cancellationToken);
    }
}
