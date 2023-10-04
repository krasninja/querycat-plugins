using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Numerology.Functions;

namespace QueryCat.Plugins.Numerology;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFunction(CalcMatrix.CalcMatrixFunction);
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
