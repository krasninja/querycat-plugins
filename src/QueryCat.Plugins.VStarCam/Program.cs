using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.VStarCam.Functions;
using QueryCat.Plugins.VStarCam.Inputs;

namespace QueryCat.Plugins.VStarCam;

/// <summary>
/// Program entry point.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        QueryCat.Plugins.Client.ThriftPluginClient.SetupApplicationLogging();
        using var client = new QueryCat.Plugins.Client.ThriftPluginClient(args);
        client.FunctionsManager.RegisterFunction(SetIr.VStarSetIrFunction);
        client.FunctionsManager.RegisterFunction(CameraInfoRowsInput.CameraInfoRowsInputFunction);
        client.FunctionsManager.RegisterFromType(typeof(CameraInfoRowsInput));
        client.FunctionsManager.RegisterFromType(typeof(CamerasRowsInput));
        await client.Start();
        await client.WaitForParentProcessExitAsync();
    }
}
