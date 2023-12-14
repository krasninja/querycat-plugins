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
internal class Program
{
    public static void QueryCatMain(ThriftPluginClientArguments args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        AsyncUtils.RunSync(async ct =>
        {
            using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
            client.FunctionsManager.RegisterFunction(SetToken.SetTokenFunction);
            client.FunctionsManager.RegisterFunction(BranchesRowsInput.GitHubBranchesFunction);
            client.FunctionsManager.RegisterFunction(CommitsRefRowsInput.GitHubCommitsRefFunction);
            client.FunctionsManager.RegisterFunction(CommitsRowsInput.GitHubCommitsFunction);
            client.FunctionsManager.RegisterFunction(IssueCommentsRowsInput.IssueCommentsFunction);
            client.FunctionsManager.RegisterFunction(PullRequestCommentsRowsInput.PullRequestCommentsFunction);
            client.FunctionsManager.RegisterFunction(PullRequestsRowsInput.GitHubPullsFunction);
            client.FunctionsManager.RegisterFunction(SearchIssuesRowsInput.GitHubSearchIssuesFunction);
            await client.StartAsync(ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
