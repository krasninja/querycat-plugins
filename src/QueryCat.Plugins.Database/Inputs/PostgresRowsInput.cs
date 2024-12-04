using System.ComponentModel;
using System.Data.Common;
using Npgsql;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Inputs;

internal sealed class PostgresRowsInput : TableRowsInput
{
    private readonly NpgsqlDataSource _dataSource;

    /// <inheritdoc />
    public PostgresRowsInput(string connectionString, string tableName, string? @namespace = null)
        : base(connectionString, tableName, @namespace)
    {
        Namespace = !string.IsNullOrEmpty(@namespace) ? @namespace : "public";
        _dataSource = NpgsqlDataSource.Create(ConnectionString);
    }

    [SafeFunction]
    [Description("Returns data from Postgres database table.")]
    [FunctionSignature("pg_table(cs: string, table: string, schema: string := ''): object<IRowsInput>")]
    public static VariantValue PostgresTableFunction(IExecutionThread thread)
    {
        var connectionString = thread.Stack[0].AsString;
        var table = thread.Stack[1].AsString;
        var schema = thread.Stack[2].AsString;
        return VariantValue.CreateFromObject(new PostgresRowsInput(connectionString, table, schema));
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

    /// <inheritdoc />
    protected override DbCommand CreateSelectAllCommand()
    {
        var query = $"SELECT * FROM {Quote(Namespace)}.{Quote(TableName)}";
        return _dataSource.CreateCommand(query);
    }

    private static string Quote(string value) => $"\"" + value.Replace("\"", "\"\"") + "\"";

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _dataSource.Dispose();
    }
}
