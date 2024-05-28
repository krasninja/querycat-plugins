using System.Runtime.InteropServices;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;
using QueryCat.Plugins.Github.Functions;
using QueryCat.Plugins.Github.Inputs;

namespace QueryCat.Plugins.Github;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static void QueryCatMain(ThriftPluginClientArguments args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging(args.LogLevel);
        AsyncUtils.RunSync(async ct =>
        {
            using var client = new ThriftPluginClient(args);
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
        functionsManager.RegisterFunction(SetToken.SetTokenFunction);
        functionsManager.RegisterFunction(BranchesRowsInput.GitHubBranchesFunction);
        functionsManager.RegisterFunction(CommitsRefRowsInput.GitHubCommitsRefFunction);
        functionsManager.RegisterFunction(CommitsRowsInput.GitHubCommitsFunction);
        functionsManager.RegisterFunction(IssueCommentsRowsInput.IssueCommentsFunction);
        functionsManager.RegisterFunction(PullRequestCommentsRowsInput.PullRequestCommentsFunction);
        functionsManager.RegisterFunction(PullRequestsRowsInput.GitHubPullsFunction);
        functionsManager.RegisterFunction(SearchIssuesRowsInput.GitHubSearchIssuesFunction);
    }
}
