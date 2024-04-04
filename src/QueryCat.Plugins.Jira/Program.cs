using System.Runtime.InteropServices;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;
using QueryCat.Plugins.Jira.Functions;
using QueryCat.Plugins.Jira.Inputs;

namespace QueryCat.Plugins.Jira;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static void QueryCatMain(ThriftPluginClientArguments args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        AsyncUtils.RunSync(async ct =>
        {
            using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
            RegisterFunctions(client.FunctionsManager);
            await client.StartAsync(ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));

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
