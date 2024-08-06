using System.Runtime.InteropServices;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;
using QueryCat.Plugins.FileSystem.Functions;
using QueryCat.Plugins.FileSystem.Inputs;

namespace QueryCat.Plugins.FileSystem;

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
            using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
            client.FunctionsManager.RegisterFunction(GetDir.GetDirFunction);
            client.FunctionsManager.RegisterFunction(FilesRowsInput.FilesFunction);
            await client.StartAsync(cancellationToken: ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
