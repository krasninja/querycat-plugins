using System.Data;
using System.Data.Common;
using System.Text;
using Npgsql;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class PostgresTableRowsProvider : TableRowsProvider, IDisposable
{
    private readonly string[] _table;
    private readonly NpgsqlDataSource _dataSource;

    public string QuotedTableName { get; }

    public PostgresTableRowsProvider(string connectionString, string table)
    {
        _table = table.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        _dataSource = NpgsqlDataSource.Create(connectionString);

        QuotedTableName = Quote(table);
    }

    /// <inheritdoc />
    public override async Task<DbDataReader> CreateDatabaseSelectReaderAsync(
        Column[] selectColumns,
        IReadOnlyList<TableSelectCondition> conditions,
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
            .Append($"{IdentityColumnName} bigint GENERATED ALWAYS AS IDENTITY NOT NULL, ")
            .Append($"CONSTRAINT {Quote("qc_" + IdentityColumnName + "_" + _table[^1])} PRIMARY KEY ({Quote(IdentityColumnName)}));");

        await using var createDatabaseTableCommand = _dataSource.CreateCommand(sb.ToString());
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

        await using var createIndexCommand = _dataSource.CreateCommand(sb.ToString());
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

        await using var createDatabaseColumnCommand = _dataSource.CreateCommand(sb.ToString());
        await createDatabaseColumnCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<long[]> InsertDatabaseRowsAsync(
        TableRowsModification[] data,
        CancellationToken cancellationToken = default)
    {
        await using var insertCommand = _dataSource.CreateCommand();

        var sb = new StringBuilder();
        foreach (var item in data)
        {
            sb.Append($"INSERT INTO {QuotedTableName} (");
            AppendSelectColumnsBlock(sb, item.Columns);
            sb.Append(") SELECT ");
            AppendSelectValuesBlock(sb, insertCommand, item.Values);
            if (item.Keys.Any())
            {
                sb.Append($" WHERE NOT EXISTS (SELECT 1 FROM {QuotedTableName} WHERE ");
                AppendWhereConditionsBlock(sb, insertCommand, item);
                sb.Append(") ");
            }
            sb.Append($"RETURNING {Quote(IdentityColumnName)};");
        }

        var query = sb.ToString();
#pragma warning disable CA2100
        insertCommand.CommandText = query;
#pragma warning restore CA2100
        return await ExecuteScalarsAsync(insertCommand, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask UpdateDatabaseRowAsync(
        TableRowsModification[] data,
        CancellationToken cancellationToken = default)
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
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var restrictions = new string?[3];
        if (_table.Length == 1)
        {
            restrictions[2] = _table[0];
        }
        if (_table.Length == 2)
        {
            restrictions[1] = _table[0];
            restrictions[2] = _table[1];
        }
        using var dt = await connection.GetSchemaAsync("Columns", restrictions, cancellationToken); // database, namespace, table.
        await connection.CloseAsync();
        return dt
            .AsEnumerable()
            .Select(r => new Column((string)r["column_name"], GetQueryCatDataType((string)r["data_type"])))
            .ToArray();
    }

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
            DataType.Numeric => "numeric",
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
            "bit" => DataType.Boolean,
            _ => DataType.String,
        };

    /// <inheritdoc />
    public void Dispose()
    {
        _dataSource.Dispose();
    }
}
