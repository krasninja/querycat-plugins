using QueryCat.Backend.Core.Functions;

namespace QueryCat.Plugins.PostgresSniffer;

internal static class Registration
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
