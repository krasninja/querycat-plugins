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
    public override DbDataReader CreateDatabaseSelectReader(Column[] columns)
    {
        var query = $"SELECT * FROM {Quote(_table)}";
        return _dataSource.CreateCommand(query).ExecuteReader();
    }

    /// <inheritdoc />
    public override void DeleteDatabaseRow(long id)
    {
        var query = $"DELETE FROM {QuotedTableName} WHERE {Quote(IdentityColumnName)} = @p0;";
        var command = _dataSource.CreateCommand(query);
        command.Parameters.AddWithValue("@p0", id);
        command.ExecuteReader();
    }

    /// <inheritdoc />
    public override void CreateDatabaseTable()
    {
        var sb = new StringBuilder()
            .Append($"CREATE TABLE IF NOT EXISTS {QuotedTableName} (")
            .Append($"{IdentityColumnName} bigint GENERATED ALWAYS AS IDENTITY NOT NULL, ")
            .Append($"CONSTRAINT {Quote("qc" + IdentityColumnName + "_" + _table[^1])} PRIMARY KEY ({Quote(IdentityColumnName)}));");

        var query = sb.ToString();
        _dataSource.CreateCommand(query).ExecuteNonQuery();
    }

    /// <inheritdoc />
    public override void CreateDatabaseUniqueKeysIndex(Column[] keyColumns)
    {
        var indexName = "qc_" + string.Join("_", keyColumns.Select(c => c.Name));
        var sb = new StringBuilder()
            .Append($"CREATE UNIQUE INDEX IF NOT EXISTS {Quote(indexName)} ON {QuotedTableName} (")
            .Append(
                string.Join(",", keyColumns.Select(k => Quote(k.Name)))
            )
            .Append(");");

        _dataSource.CreateCommand(sb.ToString()).ExecuteNonQuery();
    }

    /// <inheritdoc />
    public override void CreateDatabaseColumn(Column column)
    {
        var sb = new StringBuilder();
        sb.Append($"ALTER TABLE {QuotedTableName} ADD COLUMN IF NOT EXISTS {Quote(column.Name)} {GetDatabaseDataType(column.DataType)} NULL;");
        if (!string.IsNullOrEmpty(column.Description))
        {
            sb.Append($"COMMENT ON COLUMN {QuotedTableName}.{Quote(column.Name)} IS '{column.Description.Replace("'", "''")}';");
        }
        _dataSource.CreateCommand(sb.ToString()).ExecuteNonQuery();
    }

    /// <inheritdoc />
    public override long InsertDatabaseRow(TableRowsModification data)
    {
        var sb = new StringBuilder()
            .Append($"INSERT INTO {QuotedTableName} (")
            .Append(
                string.Join(',', data.Columns.Select(c => $"{Quote(c.Name)}"))
            )
            .Append(") SELECT ")
            .Append(
                string.Join(',', data.Columns.Select((c, i) => $"@p{i}"))
            );
        if (data.Keys.Any())
        {
            sb.Append($" WHERE NOT EXISTS (SELECT 1 FROM {QuotedTableName} WHERE ")
                .Append(
                    string.Join(" AND ", data.KeyColumns.Select((k, i) => $"{Quote(k.Name)} = @k{i}")))
                .Append(") ");
        }
        sb.Append($"RETURNING {Quote(IdentityColumnName)};");

        var insertCommand = _dataSource.CreateCommand(sb.ToString());

        for (var i = 0; i < data.Values.Length; i++)
        {
            insertCommand.Parameters.AddWithValue("p" + i, NormalizeValue(data.Values[i]));
        }
        for (var i = 0; i < data.Keys.Length; i++)
        {
            insertCommand.Parameters.AddWithValue("k" + i, NormalizeValue(data.Keys[i]));
        }
        return insertCommand.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public override void UpdateDatabaseRow(TableRowsModification data)
    {
        var sb = new StringBuilder()
            .Append($"UPDATE {QuotedTableName} SET ")
            .Append(
                string.Join(',', data.Columns.Select((c, i) =>
                    $"{Quote(c.Name)} = @p{i}")))
            .Append(" WHERE ")
            .Append(
                string.Join(" AND ", data.KeyColumns.Select((k, i) =>
                    $"{Quote(k.Name)} = @k{i}")));

        var updateCommand = _dataSource.CreateCommand(sb.ToString());
        for (var i = 0; i < data.Values.Length; i++)
        {
            updateCommand.Parameters.AddWithValue("p" + i, NormalizeValue(data.Values[i]));
        }
        for (var i = 0; i < data.Keys.Length; i++)
        {
            updateCommand.Parameters.AddWithValue("k" + i, NormalizeValue(data.Keys[i]));
        }
        updateCommand.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public override Column[] GetDatabaseTableColumns()
    {
        using var connection = _dataSource.CreateConnection();
        connection.Open();
        using var dt = connection.GetSchema("Columns", _table.Prepend(null).ToArray()); // database, namespace, table.
        return dt
            .AsEnumerable()
            .Select(r => new Column((string)r["column_name"], GetQueryCatDataType((string)r["data_type"])))
            .ToArray();
    }

    internal static string Quote(params ReadOnlySpan<string> identifiers)
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
    public void Dispose()
    {
        _dataSource.Dispose();
    }
}
