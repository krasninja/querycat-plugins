using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Clipboard.Functions;

namespace QueryCat.Plugins.Clipboard;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFunction(Get.GetFunction);
        client.FunctionsManager.RegisterFunction(Set.SetFunction);
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
