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
            client.FunctionsManager.RegisterFunction(SetBasicAuth.JiraSetBasicAuthFunction);
            client.FunctionsManager.RegisterFunction(SetToken.JiraSetTokenAuthFunction);
            client.FunctionsManager.RegisterFunction(SetUrl.JiraSetUrlFunction);
            client.FunctionsManager.RegisterFromType(typeof(IssueCommentsRowsInput));
            client.FunctionsManager.RegisterFromType(typeof(IssuesRowsInput));
            client.FunctionsManager.RegisterFromType(typeof(IssuesSearchRowsInput));
            await client.StartAsync(ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
