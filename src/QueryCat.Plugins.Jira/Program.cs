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
        client.FunctionsManager.RegisterFromType(typeof(SetBasicAuth));
        client.FunctionsManager.RegisterFromType(typeof(SetToken));
        client.FunctionsManager.RegisterFromType(typeof(SetUrl));
        client.FunctionsManager.RegisterFromType(typeof(IssueCommentsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(IssuesRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(IssuesSearchRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
