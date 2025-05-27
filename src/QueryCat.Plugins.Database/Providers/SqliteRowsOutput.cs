using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class SqliteRowsOutput : TableRowsOutput
{
    [Description("Writes data to SQLite database table.")]
    [FunctionSignature("sqlite_table_out(cs: string, table: string, keys: string = '', skip_updates: bool := false): object<IRowsOutput>")]
    public static VariantValue SqliteTableOutFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        var keys = thread.Stack[2].AsString;
        var skipUpdates = thread.Stack[3].AsBoolean;
        return VariantValue.CreateFromObject(new SqliteRowsOutput(connectionString, table, skipUpdates, keys));
    }

    /// <inheritdoc />
    public SqliteRowsOutput(string connectionString, string tableName, bool skipUpdatesIfExists, string? keys = null)
        : base(new SqliteTableRowsProvider(connectionString, tableName), skipUpdatesIfExists, tableName, keys)
    {
    }
}
