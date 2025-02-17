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

internal sealed class DuckDBTableRowsProvider : TableRowsProvider, IDisposable
{
    private readonly string[] _table;
    private readonly DuckDBConnection _dataSource;
    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(DuckDBTableRowsProvider));

    public string QuotedTableName { get; }

    private string SequenceName => $"\"seq_{IdentityColumnName}_{_table[^1]}\"";

    public DuckDBTableRowsProvider(string connectionString, string table)
    {
        _table = table.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        _dataSource = new DuckDBConnection(connectionString);

        QuotedTableName = Quote(table);
    }

    /// <inheritdoc />
    public override async Task<DbDataReader> CreateDatabaseSelectReaderAsync(Column[] selectColumns, IReadOnlyList<TableSelectCondition> conditions,
        CancellationToken cancellationToken = default)
    {
        await using var selectCommand = _dataSource.CreateCommand();

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
        await _dataSource.OpenAsync(cancellationToken);
        return await selectCommand.ExecuteReaderAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask DeleteDatabaseRowAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var deleteCommand = _dataSource.CreateCommand();
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

        await using var createDatabaseTableCommand = _dataSource.CreateCommand();
#pragma warning disable CA2100
        createDatabaseTableCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
        await _dataSource.OpenAsync(cancellationToken);
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

        await using var createIndexCommand = _dataSource.CreateCommand();
#pragma warning disable CA2100
        createIndexCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
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

        await using var createDatabaseColumnCommand = _dataSource.CreateCommand();
#pragma warning disable CA2100
        createDatabaseColumnCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
        await createDatabaseColumnCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<long[]> InsertDatabaseRowsAsync(TableRowsModification[] data, CancellationToken cancellationToken = default)
    {
        await using var insertCommand = _dataSource.CreateCommand();

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
    public override async ValueTask UpdateDatabaseRowAsync(TableRowsModification[] data, CancellationToken cancellationToken = default)
    {
        await using var updateCommand = _dataSource.CreateCommand();

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
        using var dt = await _dataSource.GetSchemaAsync("Columns", cancellationToken);
        var result = dt.AsEnumerable();
        if (_table.Length == 1)
        {
            result = result.Where(r => (string)r["table_name"] == _table[0]);
        }
        if (_table.Length == 2)
        {
            result = result.Where(r => (string)r["schema_name"] == _table[0] && (string)r["table_name"] == _table[1]);
        }
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

    /// <inheritdoc />
    public void Dispose()
    {
        _dataSource.Dispose();
    }
}
