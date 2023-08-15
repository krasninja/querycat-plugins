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
        client.FunctionsManager.RegisterFromType(typeof(Get));
        client.FunctionsManager.RegisterFromType(typeof(Set));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
