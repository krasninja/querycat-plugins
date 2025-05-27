using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class SqliteRowsInput : TableRowsInput
{
    [SafeFunction]
    [Description("Returns data from SQLite database table.")]
    [FunctionSignature("sqlite_table(cs: string, table: string): object<IRowsInput>")]
    public static VariantValue SqliteTableFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        return VariantValue.CreateFromObject(new SqliteRowsInput(connectionString, table));
    }

    /// <inheritdoc />
    public SqliteRowsInput(string connectionString, string tableName)
        : base(new SqliteTableRowsProvider(connectionString, tableName), tableName)
    {
    }
}
