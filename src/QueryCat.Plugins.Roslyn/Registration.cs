using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Roslyn.Functions;

namespace QueryCat.Plugins.Roslyn;

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
        functionsManager.RegisterFunction(InitializeBuildHost.InitializeBuildHostFunction);
        functionsManager.RegisterFunctions(
            functionsManager.Factory.CreateFromType(typeof(Inputs.RoslynClassesRowsInput))
        );
    }
}
