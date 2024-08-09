using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Jira.Functions;
using QueryCat.Plugins.Jira.Inputs;

namespace QueryCat.Plugins.Jira;

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
        functionsManager.RegisterFunction(SetBasicAuth.JiraSetBasicAuthFunction);
        functionsManager.RegisterFunction(SetToken.JiraSetTokenAuthFunction);
        functionsManager.RegisterFunction(SetUrl.JiraSetUrlFunction);
        functionsManager.RegisterFunction(IssueCommentsRowsInput.JiraIssueCommentsFunction);
        functionsManager.RegisterFunction(IssuesRowsInput.JiraIssueFunction);
        functionsManager.RegisterFunction(IssuesSearchRowsInput.JiraIssueSearchFunction);
    }
}
