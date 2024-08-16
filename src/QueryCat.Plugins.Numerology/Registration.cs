using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Numerology.Functions;

namespace QueryCat.Plugins.Numerology;

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
        functionsManager.RegisterFunction(CalcMatrix.CalcMatrixFunction);
    }
}
