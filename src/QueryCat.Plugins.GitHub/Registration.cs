using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Github.Functions;
using QueryCat.Plugins.Github.Inputs;

namespace QueryCat.Plugins.Github;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
internal static class Registration
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
        functionsManager.RegisterFunction(IssueCommentsRowsInput.IssueCommentsFunction);
        functionsManager.RegisterFunction(IssueTimelineRowsInput.IssueTimelineFunction);
        functionsManager.RegisterFunction(PullRequestCommentsRowsInput.PullRequestCommentsFunction);
        functionsManager.RegisterFunction(PullRequestRequestedReviewsRowsInput.PullRequestedReviewsFunction);
        functionsManager.RegisterFunction(PullRequestReviewsRowsInput.PullReviewsFunction);
        functionsManager.RegisterFunction(PullRequestsRowsInput.GitHubPullsFunction);
        functionsManager.RegisterFunction(SearchIssuesRowsInput.GitHubSearchIssuesFunction);
    }
}
