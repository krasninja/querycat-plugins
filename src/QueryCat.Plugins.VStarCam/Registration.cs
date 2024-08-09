using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.VStarCam.Functions;
using QueryCat.Plugins.VStarCam.Inputs;

namespace QueryCat.Plugins.VStarCam;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
internal static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(SetIr.VStarSetIrFunction);
        functionsManager.RegisterFunction(CameraInfoRowsInput.CameraInfoRowsInputFunction);
        functionsManager.RegisterFunction(CamerasRowsInput.CamerasRowsInputFunction);
    }
}
