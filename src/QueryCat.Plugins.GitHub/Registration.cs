using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Github.Functions;
using QueryCat.Plugins.Github.Inputs;

namespace QueryCat.Plugins.Github;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
public static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(SetToken.SetTokenFunction);
        functionsManager.RegisterFunction(RepositoriesRowsInput.GitHubRepositoriesFunction);
        functionsManager.RegisterFunction(BranchesRowsInput.GitHubBranchesFunction);
        functionsManager.RegisterFunction(CommitsRefRowsInput.GitHubCommitsRefFunction);
        functionsManager.RegisterFunction(CommitsRowsInput.GitHubCommitsFunction);
        functionsManager.RegisterFunction(GistsRowsInput.GitHubGistsFunction);
        functionsManager.RegisterFunction(OrganizationsRowsInput.GitHubOrganizationsFunction);
        functionsManager.RegisterFunction(OrganizationMembersRowsInput.GitHubOrganizationMembersFunction);
        functionsManager.RegisterFunction(PackagesRowsInput.GitHubPackagesFunction);
        functionsManager.RegisterFunction(PackageVersionsRowsInput.GitHubPackageVersionsFunction);
        functionsManager.RegisterFunction(IssueCommentsRowsInput.GitHubIssueCommentsFunction);
        functionsManager.RegisterFunction(IssueTimelineRowsInput.GitHubIssueTimelineFunction);
        functionsManager.RegisterFunction(PullRequestCommentsRowsInput.GitHubPullRequestCommentsFunction);
        functionsManager.RegisterFunction(PullRequestCommitsRowsInput.GitHubPullRequestCommitsFunction);
        functionsManager.RegisterFunction(PullRequestFilesRowsInput.GitHubPullRequestFilesFunction);
        functionsManager.RegisterFunction(PullRequestRequestedReviewsRowsInput.GitHubPullRequestedReviewsFunction);
        functionsManager.RegisterFunction(PullRequestReviewsRowsInput.GitHubPullReviewsFunction);
        functionsManager.RegisterFunction(PullRequestsRowsInput.GitHubPullsFunction);
        functionsManager.RegisterFunction(RateLimitsRowsInput.GitHubRateLimitsFunction);
        functionsManager.RegisterFunction(SearchIssuesRowsInput.GitHubSearchIssuesFunction);
        functionsManager.RegisterFunction(TeamsRowsInput.GitHubTeamsFunction);
        functionsManager.RegisterFunction(RepositoryCollaboratorsRowsInput.GitHubRepositoryCollaboratorsFunction);
        functionsManager.RegisterFunction(RepositoryContentsRowsInput.GitHubRepositoryContentsFunction);
        functionsManager.RegisterFunction(RepositoryDeploymentsRowsInput.GitHubRepositoryDeploymentsFunction);
        functionsManager.RegisterFunction(RepositoryEnvironmentsRowsInput.GitHubRepositoryEnvironmentsFunction);
        functionsManager.RegisterFunction(TagsRowsInput.GitHubTagsFunction);
        functionsManager.RegisterFunction(TeamMembersRowsInput.GitHubTeamMembersFunction);
        functionsManager.RegisterFunction(TeamRepositoriesRowsInput.GitHubTeamRepositoriesFunction);
        functionsManager.RegisterFunction(TreeRowsInput.GitHubTreeFunction);
        functionsManager.RegisterFunction(UsersRowsInput.GitHubUserFunction);
        functionsManager.RegisterFunction(WorkflowsRowsInput.GitHubWorkflowsFunction);
    }
}
