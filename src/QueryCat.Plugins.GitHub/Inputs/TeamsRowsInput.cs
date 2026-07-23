using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub organization teams input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/teams/teams#list-teams.
/// </remarks>
internal sealed class TeamsRowsInput : BaseRowsInput<Team>
{
    [SafeFunction]
    [Description("Return GitHub teams of specific organization.")]
    [FunctionSignature("github_teams(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubTeamsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new TeamsRowsInput(token));
    }

    public TeamsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Team> builder)
    {
        builder
            .AddProperty("organization", DataType.String, _ => GetKeyColumnValue("organization"), "The organization login name.")
            .AddProperty("id", p => p.Id, "Team id.")
            .AddProperty("node_id", p => p.NodeId, "Team node id.")
            .AddProperty("name", p => p.Name, "Team name.")
            .AddProperty("slug", p => p.Slug, "Team slug.")
            .AddProperty("description", p => p.Description, "Team description.")
            .AddProperty("privacy", p => p.Privacy.Value, "Team privacy (secret or closed).")
            .AddProperty("permission", p => p.Permission, "Team permission.")
            .AddProperty("members_count", p => p.MembersCount, "Number of team members.")
            .AddProperty("repos_count", p => p.ReposCount, "Number of repositories.")
            .AddProperty("url", p => p.Url, "Team URL.")
            .AddKeyColumn("organization", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Team> GetDataAsync(Fetcher<Team> fetcher, CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization");

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            await Client.Organization.Team.GetAll(organization,
                new ApiOptions
                {
                    StartPage = page,
                    PageCount = 1,
                    PageSize = limit,
                }),
            cancellationToken);
    }
}
