using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.System.Inputs;

namespace QueryCat.Plugins.System;

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
        functionsManager.RegisterFunction(ArgsRowsInput.ArgsRowsFunction);
        functionsManager.RegisterFunction(EnvsRowsInput.EnvsRowsFunction);
        functionsManager.RegisterFunction(ProcessesRowsInput.ProcessesFunction);
    }
}
