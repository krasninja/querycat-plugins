using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.System.Inputs;

namespace QueryCat.Plugins.System;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFromType(typeof(ArgsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(EnvsRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(ProcessesRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
