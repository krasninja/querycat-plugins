using System.Runtime.InteropServices;
using Octokit;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;
using QueryCat.Plugins.Github.Functions;
using QueryCat.Plugins.Github.Inputs;

namespace QueryCat.Plugins.Github;

/// <summary>
/// Program entry point.
/// </summary>
internal class Program
{
    public static void QueryCatMain(ThriftPluginClientArguments args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        AsyncUtils.RunSync(async () =>
        {
            using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
            client.FunctionsManager.RegisterFunction(SetToken.SetTokenFunction);
            client.FunctionsManager.RegisterFunction(BranchesRowsInput.GitHubBranchesFunction);
            client.FunctionsManager.RegisterFromType(typeof(BranchesRowsInput));
            client.FunctionsManager.RegisterFunction(CommitsRefRowsInput.GitHubCommitsRefFunction);
            client.FunctionsManager.RegisterFromType(typeof(CommitsRefRowsInput));
            client.FunctionsManager.RegisterFunction(CommitsRowsInput.GitHubCommitsFunction);
            client.FunctionsManager.RegisterFromType(typeof(CommitsRowsInput));
            client.FunctionsManager.RegisterFromType(typeof(IssueCommentsRowsInput));
            client.FunctionsManager.RegisterFromType(typeof(PullRequestCommentsRowsInput));
            client.FunctionsManager.RegisterFunction(PullRequestsRowsInput.GitHubPullsFunction);
            client.FunctionsManager.RegisterFromType(typeof(PullRequestsRowsInput));
            client.FunctionsManager.RegisterFunction(SearchIssuesRowsInput.GitHubSearchIssuesFunction);
            client.FunctionsManager.RegisterFromType(typeof(SearchIssuesRowsInput));
            await client.StartAsync();
            await client.WaitForServerExitAsync();
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
