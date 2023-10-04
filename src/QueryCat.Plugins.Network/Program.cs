using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Network.Functions;
using QueryCat.Plugins.Network.Inputs;

namespace QueryCat.Plugins.Network;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFunction(Hostname.HostnameFunction);
        client.FunctionsManager.RegisterFunction(IntToIp.IntToIpFunction);
        client.FunctionsManager.RegisterFunction(IpToInt.IpToIntFunction);
        client.FunctionsManager.RegisterFromType(typeof(InterfaceAddressesRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(InterfaceDetailsRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
