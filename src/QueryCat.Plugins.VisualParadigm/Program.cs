﻿using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Utils;
using QueryCat.Plugins.Client;

namespace QueryCat.Plugins.VisualParadigm;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static void QueryCatMain(ThriftPluginClientArguments args)
    {
        ThriftPluginClient.SetupApplicationLogging(logLevel: LogLevel.Debug);
        AsyncUtils.RunSync(async ct =>
        {
            using var client = new ThriftPluginClient(args);
            await client.StartAsync(ct);
            await client.WaitForServerExitAsync(ct);
        });
    }

    [UnmanagedCallersOnly(EntryPoint = ThriftPluginClient.PluginMainFunctionName)]
    public static void DllMain(QueryCatPluginArguments args) => QueryCatMain(args.ConvertToPluginClientArguments());

    public static void Main(string[] args) => QueryCatMain(ThriftPluginClient.ConvertCommandLineArguments(args));
}
