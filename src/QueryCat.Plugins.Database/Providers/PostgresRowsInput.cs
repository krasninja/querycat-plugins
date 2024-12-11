using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class PostgresRowsInput : TableRowsInput
{
    /// <inheritdoc />
    public PostgresRowsInput(string connectionString, string tableName)
        : base(new PostgresTableRowsProvider(connectionString, tableName), tableName)
    {
    }

    [SafeFunction]
    [Description("Returns data from Postgres database table.")]
    [FunctionSignature("pg_table(cs: string, table: string): object<IRowsInput>")]
    public static VariantValue PostgresTableFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        return VariantValue.CreateFromObject(new PostgresRowsInput(connectionString, table));
    }
}
