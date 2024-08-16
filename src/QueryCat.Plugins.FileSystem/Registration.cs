using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.FileSystem.Functions;
using QueryCat.Plugins.FileSystem.Inputs;

namespace QueryCat.Plugins.FileSystem;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
public static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(GetDir.GetDirFunction);
        functionsManager.RegisterFunction(FilesRowsInput.FilesFunction);
    }
}
