using QueryCat.Plugins.Aws.Functions;
using QueryCat.Plugins.Aws.Inputs;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFromType(typeof(SetToken));
        client.FunctionsManager.RegisterFromType<Ec2InstancesRowsInput>();
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
