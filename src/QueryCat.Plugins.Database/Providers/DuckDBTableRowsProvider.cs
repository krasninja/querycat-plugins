using System.Data;
using System.Data.Common;
using System.Text;
using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class DuckDBTableRowsProvider : TableRowsProvider
{
    private readonly string _connectionString;
    private readonly string[] _table;
    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(DuckDBTableRowsProvider));

    public string QuotedTableName { get; }

    private string SequenceName => $"\"seq_{IdentityColumnName}_{_table[^1]}\"";

    public DuckDBTableRowsProvider(string connectionString, string table)
    {
        _connectionString = connectionString;
        _table = table.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        QuotedTableName = Quote(table);
    }

    /// <inheritdoc />
    public override Task OpenAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private DbCommand CreateCommand(DuckDBConnection dataSource, string? query = null)
    {
        var command = dataSource.CreateCommand();
        if (!string.IsNullOrEmpty(query))
        {
#pragma warning disable CA2100
            command.CommandText = query;
#pragma warning restore CA2100
        }
        return command;
    }

    /// <inheritdoc />
    public override async Task<DbDataReader> CreateDatabaseSelectReaderAsync(Column[] selectColumns, IReadOnlyList<TableSelectCondition> conditions,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new DuckDBConnection(_connectionString);
        await using var selectCommand = CreateCommand(connection);

        var sb = new StringBuilder("SELECT ");
        AppendSelectColumnsBlock(sb, selectColumns);
        sb.Append($" FROM {Quote(_table)}");
        if (conditions.Count > 0)
        {
            sb.Append(" WHERE ");
            AppendWhereConditionsBlock(sb, selectCommand, conditions);
        }
#pragma warning disable CA2100
        selectCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
        return await selectCommand.ExecuteReaderAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask DeleteDatabaseRowAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new DuckDBConnection(_connectionString);
        await using var deleteCommand = CreateCommand(connection);
        var sb = new StringBuilder($"DELETE FROM {QuotedTableName} WHERE ");
        AppendWhereConditionsBlock(sb, deleteCommand, new TableSelectCondition(IdentityColumn, new VariantValue(id)));
#pragma warning disable CA2100
        deleteCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task CreateDatabaseTableAsync(CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder()
            .Append($"CREATE TABLE IF NOT EXISTS {QuotedTableName} (")
            .Append($"{IdentityColumnName} bigint NOT NULL PRIMARY KEY);")
            .Append($"CREATE SEQUENCE IF NOT EXISTS {SequenceName} START 1;");

        await using var connection = new DuckDBConnection(_connectionString);
        await using var createDatabaseTableCommand = CreateCommand(connection, sb.ToString());
        await createDatabaseTableCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task CreateDatabaseUniqueKeysIndexAsync(Column[] keyColumns, CancellationToken cancellationToken = default)
    {
        var indexName = "qc_" + string.Join("_", keyColumns.Select(c => c.Name));
        var sb = new StringBuilder()
            .Append($"CREATE UNIQUE INDEX IF NOT EXISTS {Quote(indexName)} ON {QuotedTableName} (")
            .Append(
                string.Join(",", keyColumns.Select(k => Quote(k.Name)))
            )
            .Append(");");

        await using var connection = new DuckDBConnection(_connectionString);
        await using var createIndexCommand = CreateCommand(connection, sb.ToString());
        await createIndexCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task CreateDatabaseColumnAsync(Column column, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.Append($"ALTER TABLE {QuotedTableName} ADD COLUMN IF NOT EXISTS {Quote(column.Name)} {GetDatabaseDataType(column.DataType)} NULL;");
        if (!string.IsNullOrEmpty(column.Description))
        {
            sb.Append($"COMMENT ON COLUMN {QuotedTableName}.{Quote(column.Name)} IS '{column.Description.Replace("'", "''")}';");
        }

        await using var connection = new DuckDBConnection(_connectionString);
        await using var createDatabaseColumnCommand = CreateCommand(connection, sb.ToString());
        await createDatabaseColumnCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<long[]> InsertDatabaseRowsAsync(TableRowsModification[] data, CancellationToken cancellationToken = default)
    {
        await using var connection = new DuckDBConnection(_connectionString);
        await using var insertCommand = CreateCommand(connection);

        var sb = new StringBuilder();
        foreach (var item in data)
        {
            sb.Append($"INSERT INTO {QuotedTableName} (");
            sb.Append(IdentityColumnName + ",");
            AppendSelectColumnsBlock(sb, item.Columns);
            sb.Append(") SELECT ");
            sb.Append($"nextval('{SequenceName}'),");
            AppendSelectValuesBlock(sb, insertCommand, item.Values);
            if (item.Keys.Any())
            {
                sb.Append($" WHERE NOT EXISTS (SELECT 1 FROM {QuotedTableName} WHERE ");
                AppendWhereConditionsBlock(sb, insertCommand, item);
                sb.Append(") ");
            }
            sb.Append($" RETURNING {Quote(IdentityColumnName)};");
        }

        var query = sb.ToString();
#pragma warning disable CA2100
        insertCommand.CommandText = query;
#pragma warning restore CA2100
        return await ExecuteScalarsAsync(insertCommand, cancellationToken);
    }

    /// <inheritdoc />
    protected override void AppendSelectValuesBlock(StringBuilder sb, DbCommand command, params VariantValue[] values)
    {
        var prefix = GetNextId();
        sb.Append(
            string.Join(',', values.Select((v, i) =>
                string.Concat(
                    FormatParameterName(prefix, i, "v"),
                    "::",
                    GetDatabaseDataType(v.Type))
                )
            )
        );
        for (var i = 0; i < values.Length; i++)
        {
            var param = command.CreateParameter();
            param.ParameterName = FormatParameterNamePlain(prefix, i, "v");
            param.Value = NormalizeValue(values[i]);
            command.Parameters.Add(param);
        }
    }

    /// <inheritdoc />
    protected override void AppendWhereConditionsBlock(StringBuilder sb, DbCommand command, params IReadOnlyList<TableSelectCondition> conditions)
    {
        string FormatParameterNameWithType(string prefix, int i, string type, VariantValue value)
        {
            return string.Concat(
                FormatParameterName(prefix, i, "cv"),
                "::",
                GetDatabaseDataType(value.Type)
            );
        }

        var prefix = GetNextId();
        sb.Append(
            string.Join(" AND ",
                conditions.Select((c, i)
                    => $"{Quote(c.Column.Name)} {GetOperationString(c.Operation)} {FormatParameterNameWithType(prefix, i, "cv", c.Value)}")
            )
        );
        for (var i = 0; i < conditions.Count; i++)
        {
            var param = command.CreateParameter();
            param.ParameterName = FormatParameterNamePlain(prefix, i, "cv");
            param.Value = NormalizeValue(conditions[i].Value);
            command.Parameters.Add(param);
        }
    }

    /// <inheritdoc />
    public override async ValueTask UpdateDatabaseRowAsync(TableRowsModification[] data, CancellationToken cancellationToken = default)
    {
        await using var connection = new DuckDBConnection(_connectionString);
        await using var updateCommand = CreateCommand(connection);

        var sb = new StringBuilder();
        foreach (var item in data)
        {
            sb.Append($"UPDATE {QuotedTableName} SET ");
            AppendUpdateValuesBlock(sb, updateCommand, item);
            sb.Append(" WHERE ");
            AppendWhereConditionsBlock(sb, updateCommand, item);
            sb.Append(';');
        }

#pragma warning disable CA2100
        updateCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<Column[]> GetDatabaseTableColumnsAsync(CancellationToken cancellationToken = default)
    {
        var restrictions = new[]
        {
            null, // table_catalog
            _table.Length == 2 ? _table[0] : null, // table_schema
            _table[^1], // table_name
            null, // column_name
        };
        await using var connection = new DuckDBConnection(_connectionString);
        using var dt = await connection.GetSchemaAsync("Columns", restrictions, cancellationToken);
        var result = dt.AsEnumerable();
        return result.Select(r => new Column((string)r["column_name"], GetQueryCatDataType((string)r["data_type"])))
            .ToArray();
    }

    /// <inheritdoc />
    protected override string FormatParameterName(string name) => "$" + base.FormatParameterName(name);

    /// <inheritdoc />
    protected override string Quote(params ReadOnlySpan<string> identifiers)
    {
        if (identifiers.IsEmpty)
        {
            return string.Empty;
        }

        // Check if it is already quoted.
        var sb = new StringBuilder();
        foreach (var id in identifiers)
        {
            if (id.StartsWith('\"'))
            {
                sb.Append(id);
            }
            else
            {
                sb.Append('"')
                    .Append(id.Replace("\"", "\"\""))
                    .Append('"');
            }
        }
        return sb.ToString();
    }

    private static string GetDatabaseDataType(DataType type)
        => type switch
        {
            DataType.Integer => "bigint",
            DataType.Numeric => "decimal",
            DataType.String => "text",
            DataType.Timestamp => "timestamp",
            DataType.Float => "double",
            DataType.Boolean => "boolean",
            _ => "text",
        };

    private static DataType GetQueryCatDataType(string type)
        => type switch
        {
            "integer" => DataType.Integer,
            "int" => DataType.Integer,
            "smallint" => DataType.Integer,
            "bigint" => DataType.Integer,
            "text" => DataType.String,
            "varchar" => DataType.String,
            "date" => DataType.Timestamp,
            "timestamp" => DataType.Timestamp,
            "double" => DataType.Float,
            "numeric" => DataType.Numeric,
            "boolean" => DataType.Boolean,
            _ => DataType.String,
        };
}
