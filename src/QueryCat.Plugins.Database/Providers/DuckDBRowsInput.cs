using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class DuckDBRowsInput : TableRowsInput
{
    [SafeFunction]
    [Description("Returns data from DuckDB database table.")]
    [FunctionSignature("duckdb_table(cs: string, table: string): object<IRowsInput>")]
    public static VariantValue DuckDBTableFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        return VariantValue.CreateFromObject(new DuckDBRowsInput(connectionString, table));
    }

    /// <inheritdoc />
    public DuckDBRowsInput(string connectionString, string tableName)
        : base(new DuckDBTableRowsProvider(connectionString, tableName), tableName)
    {
    }
}
