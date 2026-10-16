using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub team repositories input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/teams/teams#list-team-repositories.
/// </remarks>
internal sealed class TeamRepositoriesRowsInput : BaseRowsInput<Repository>
{
    [SafeFunction]
    [Description("Return GitHub repositories of a specific team.")]
    [FunctionSignature("github_team_repositories(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubTeamRepositoriesFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new TeamRepositoriesRowsInput(token));
    }

    public TeamRepositoriesRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Repository> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_team_repository.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("organization", DataType.String, _ => GetKeyColumnValue("organization"), "The organization login name.")
            .AddProperty("team_slug", DataType.String, _ => GetKeyColumnValue("team_slug"), "The team slug name.")
            .AddProperty("id", p => p.Id, "The unique ID of the repository.")
            .AddProperty("name", p => p.Name, "The name of the repository.")
            .AddProperty("full_name", p => p.FullName, "The full name of the repository (owner/repo).")
            .AddProperty("description", p => p.Description, "The description of the repository.")
            .AddProperty("private", p => p.Private, "True if the repository is private.")
            .AddProperty("fork", p => p.Fork, "True if the repository is a fork.")
            .AddProperty("archived", p => p.Archived, "True if the repository is archived.")
            .AddProperty("language", p => p.Language, "The primary language of the repository.")
            .AddProperty("stargazers_count", p => p.StargazersCount, "The number of stars on the repository.")
            .AddProperty("forks_count", p => p.ForksCount, "The number of forks of the repository.")
            .AddProperty("open_issues_count", p => p.OpenIssuesCount, "The number of open issues.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the repository.")
            .AddProperty("clone_url", p => p.CloneUrl, "The clone URL of the repository.")
            .AddProperty("default_branch", p => p.DefaultBranch, "The default branch of the repository.")
            .AddProperty("permissions_admin", p => p.Permissions?.Admin, "True if the team has admin access to the repository.")
            .AddProperty("permissions_push", p => p.Permissions?.Push, "True if the team has push access to the repository.")
            .AddProperty("permissions_pull", p => p.Permissions?.Pull, "True if the team has pull access to the repository.")
            .AddKeyColumn("organization", isRequired: true)
            .AddKeyColumn("team_slug", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Repository> GetDataAsync(Fetcher<Repository> fetcher, CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization").AsString;
        var teamSlug = GetKeyColumnValue("team_slug").AsString;

        return fetcher.FetchAllAsync(async ct =>
        {
            // First get the team by slug to get the team ID.
            var team = await Client.Organization.Team.GetByName(organization, teamSlug);

            // Then get all repositories of that team.
            var repositories = await Client.Organization.Team.GetAllRepositories(team.Id);
            return repositories;
        }, cancellationToken);
    }
}
