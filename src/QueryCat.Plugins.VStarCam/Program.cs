using System.Runtime.InteropServices;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;
using QueryCat.Plugins.VStarCam.Functions;
using QueryCat.Plugins.VStarCam.Inputs;

namespace QueryCat.Plugins.VStarCam;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static void QueryCatMain(ThriftPluginClientArguments args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        AsyncUtils.RunSync(async () =>
        {
            using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
            client.FunctionsManager.RegisterFunction(SetIr.VStarSetIrFunction);
            client.FunctionsManager.RegisterFunction(CameraInfoRowsInput.CameraInfoRowsInputFunction);
            client.FunctionsManager.RegisterFromType(typeof(CameraInfoRowsInput));
            client.FunctionsManager.RegisterFromType(typeof(CamerasRowsInput));
            await client.StartAsync();
            await client.WaitForServerExitAsync();
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
