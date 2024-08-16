using QueryCat.Backend.Core.Functions;

namespace QueryCat.Plugins.PostgresSniffer;

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
        functionsManager.RegisterFunction(Inputs.PostgresQueriesRowsInput.PostgresSnifferStart);
    }
}
