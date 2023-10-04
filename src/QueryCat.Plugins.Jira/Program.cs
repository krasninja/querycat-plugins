using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Jira.Functions;
using QueryCat.Plugins.Jira.Inputs;

namespace QueryCat.Plugins.Jira;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFunction(SetBasicAuth.JiraSetBasicAuthFunction);
        client.FunctionsManager.RegisterFunction(SetToken.JiraSetTokenAuthFunction);
        client.FunctionsManager.RegisterFunction(SetUrl.JiraSetUrlFunction);
        client.FunctionsManager.RegisterFromType(typeof(IssueCommentsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(IssuesRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(IssuesSearchRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
