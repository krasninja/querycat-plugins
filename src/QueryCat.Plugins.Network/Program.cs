using System.Runtime.InteropServices;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;
using QueryCat.Plugins.Network.Functions;
using QueryCat.Plugins.Network.Inputs;

namespace QueryCat.Plugins.Network;

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
            client.FunctionsManager.RegisterFunction(Hostname.HostnameFunction);
            client.FunctionsManager.RegisterFunction(IntToIp.IntToIpFunction);
            client.FunctionsManager.RegisterFunction(IpToInt.IpToIntFunction);
            client.FunctionsManager.RegisterFunction(InterfaceAddressesRowsInput.InterfaceAddressesFunction);
            client.FunctionsManager.RegisterFunction(InterfaceDetailsRowsInput.InterfaceDetailsFunction);
            await client.StartAsync(ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
