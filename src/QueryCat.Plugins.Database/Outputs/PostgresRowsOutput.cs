using System.ComponentModel;
using System.Data.Common;
using System.Text;
using Npgsql;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Outputs;

internal sealed class PostgresRowsOutput : TableRowsOutput
{
    private readonly NpgsqlDataSource _dataSource;

    [SafeFunction]
    [Description("Writes data to Postgres database table.")]
    [FunctionSignature("pg_table_out(cs: string, table: string, schema: string := ''): object<IRowsOutput>")]
    public static VariantValue PostgresTableFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        var schema = thread.Stack[2].AsString;
        return VariantValue.CreateFromObject(new PostgresRowsOutput(connectionString, table, schema));
    }

    /// <inheritdoc />
    public PostgresRowsOutput(string connectionString, string tableName, string? @namespace = null)
        : base(connectionString, tableName, @namespace)
    {
        Namespace = !string.IsNullOrEmpty(@namespace) ? @namespace : "public";
        _dataSource = NpgsqlDataSource.Create(ConnectionString);
    }

    private static string GetDatabaseDataType(DataType type)
        => type switch
        {
            DataType.Integer => "bigint",
            DataType.Numeric => "numeric",
            DataType.String => "text",
            DataType.Timestamp => "timestamp",
            DataType.Float => "double",
            DataType.Boolean => "bit",
            _ => "text",
        };

    private static DataType GetQueryCatDataType(string type)
        => type switch
        {
            "integer" => DataType.Integer,
            "int" => DataType.Integer,
            "smallint" => DataType.Integer,
            "bigint" => DataType.Integer,
            "char" => DataType.String,
            "character" => DataType.String,
            "character varying" => DataType.String,
            "text" => DataType.String,
            "guid" => DataType.String,
            "varchar" => DataType.String,
            "date" => DataType.Timestamp,
            "timestamp" => DataType.Timestamp,
            "timestamp with time zone" => DataType.Timestamp,
            "double precision" => DataType.Float,
            "real" => DataType.Float,
            "numeric" => DataType.Numeric,
            "boolean" => DataType.Boolean,
            _ => DataType.String,
        };

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    /// <inheritdoc />
    protected override DbCommand CreateCreateTableCommand()
    {
        var sb = new StringBuilder();

        sb.Append($"CREATE TABLE IF NOT EXISTS {Quote(Namespace)}.{Quote(TableName)} (");
        var columns = new List<string>();
        columns.Add($"{IdentityColumnName} bigint GENERATED ALWAYS AS IDENTITY NOT NULL");
        foreach (var column in QueryContext.QueryInfo.Columns)
        {
            columns.Add($"{Quote(column.Name)} {GetDatabaseDataType(column.DataType)} NULL");
        }
        sb.Append(string.Join(",", columns));
        sb.Append(");");
        return _dataSource.CreateCommand(sb.ToString());
    }

    /// <inheritdoc />
    protected override DbCommand CreateInsertCommand()
    {
        var sb = new StringBuilder();
        sb.Append($"INSERT INTO {Quote(Namespace)}.{Quote(TableName)} (");
        sb.Append(
            string.Join(',', QueryContext.QueryInfo.Columns.Select((c, i) => $"{Quote(c.Name)}"))
        );
        sb.Append(") VALUES (");
        sb.Append(
            string.Join(',', QueryContext.QueryInfo.Columns.Select((c, i) => $"@p{i}"))
        );
        sb.Append(")");
        return _dataSource.CreateCommand(sb.ToString());
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _dataSource.Dispose();
    }
}
