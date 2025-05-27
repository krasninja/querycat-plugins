using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Database.Providers;

namespace QueryCat.Plugins.Database;

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
        functionsManager.RegisterFunction(DuckDBRowsInput.DuckDBTableFunction);
        functionsManager.RegisterFunction(DuckDBRowsOutput.DuckDBTableOutFunction);
        functionsManager.RegisterFunction(PostgresRowsInput.PostgresTableFunction);
        functionsManager.RegisterFunction(PostgresRowsOutput.PostgresTableOutFunction);
        functionsManager.RegisterFunction(SqliteRowsInput.SqliteTableFunction);
        functionsManager.RegisterFunction(SqliteRowsOutput.SqliteTableOutFunction);
    }
}
