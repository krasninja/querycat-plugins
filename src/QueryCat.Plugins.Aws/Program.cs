using System.Runtime.InteropServices;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Aws.Functions;
using QueryCat.Plugins.Aws.Inputs;
using QueryCat.Plugins.Client;

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
            client.FunctionsManager.RegisterFunction(SetToken.AwsSetTokenAuthFunction);
            client.FunctionsManager.RegisterFromType(typeof(Ec2InstancesRowsInput));
            await client.StartAsync(ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
