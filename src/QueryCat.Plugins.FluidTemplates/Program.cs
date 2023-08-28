using QueryCat.Backend.Core.Functions;

namespace QueryCat.Plugins.FluidTemplates;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFromType(typeof(FluidTemplateRowsOutput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
