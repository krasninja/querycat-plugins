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
        functionsManager.RegisterFunction(BranchesRowsInput.GitHubBranchesFunction);
        functionsManager.RegisterFunction(CommitsRefRowsInput.GitHubCommitsRefFunction);
        functionsManager.RegisterFunction(CommitsRowsInput.GitHubCommitsFunction);
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
    }
}
