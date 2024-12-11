using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class PostgresRowsOutput : TableRowsOutput
{
    [Description("Writes data to Postgres database table.")]
    [FunctionSignature("pg_table_out(cs: string, table: string, keys: string = ''): object<IRowsOutput>")]
    public static VariantValue PostgresTableFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        var keys = thread.Stack[2].AsString;
        return VariantValue.CreateFromObject(new PostgresRowsOutput(connectionString, table, keys));
    }

    /// <inheritdoc />
    public PostgresRowsOutput(string connectionString, string tableName, string? keys = null)
        : base(new PostgresTableRowsProvider(connectionString, tableName), tableName, keys)
    {
    }
}
