using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.FileSystem.Functions;
using QueryCat.Plugins.FileSystem.Inputs;

namespace QueryCat.Plugins.FileSystem;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFromType(typeof(GetDir));
        client.FunctionsManager.RegisterFromType(typeof(FilesRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
