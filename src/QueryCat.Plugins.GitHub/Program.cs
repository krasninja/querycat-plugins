using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Github.Functions;
using QueryCat.Plugins.Github.Inputs;

namespace QueryCat.Plugins.Github;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFromType(typeof(SetToken));
        client.FunctionsManager.RegisterFromType(typeof(BranchesRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(CommitsRefRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(CommitsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(IssueCommentsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(PullRequestCommentsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(PullRequestsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(SearchIssuesRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
