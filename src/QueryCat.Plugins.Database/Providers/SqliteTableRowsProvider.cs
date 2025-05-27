using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Database.Common;

namespace QueryCat.Plugins.Database.Providers;

internal sealed class SqliteTableRowsProvider : TableRowsProvider
{
    private readonly string _connectionString;
    private readonly string[] _table;
    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(SqliteTableRowsProvider));

    public string QuotedTableName { get; }

    public SqliteTableRowsProvider(string connectionString, string table)
    {
        _connectionString = connectionString;
        _table = [table];

        QuotedTableName = Quote(table);
    }

    /// <inheritdoc />
    public override Task OpenAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private DbCommand CreateCommand(SqliteConnection dataSource, string? query = null)
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
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var selectCommand = CreateCommand(connection);

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
        return await selectCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<int> DeleteDatabaseRowAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var deleteCommand = CreateCommand(connection);
        var sb = new StringBuilder($"DELETE FROM {QuotedTableName} WHERE ");
        AppendWhereConditionsBlock(sb, deleteCommand, new TableSelectCondition(IdentityColumn, new VariantValue(id)));
#pragma warning disable CA2100
        deleteCommand.CommandText = sb.ToString();
#pragma warning restore CA2100
        return await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task CreateDatabaseTableAsync(CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder()
            .Append($"CREATE TABLE IF NOT EXISTS {QuotedTableName} (")
            .Append($"{IdentityColumnName} INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT);");

        await using var connection = new SqliteConnection(_connectionString);
        await using var createDatabaseTableCommand = CreateCommand(connection, sb.ToString());
        await connection.OpenAsync(cancellationToken);
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

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var createIndexCommand = CreateCommand(connection, sb.ToString());
        await createIndexCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task CreateDatabaseColumnAsync(Column column, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.Append($"ALTER TABLE {QuotedTableName} ADD COLUMN {Quote(column.Name)} {GetDatabaseDataType(column.DataType)} NULL;");

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var createDatabaseColumnCommand = CreateCommand(connection, sb.ToString());
        await createDatabaseColumnCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<long[]> InsertDatabaseRowsAsync(TableRowsModification[] data, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var insertCommand = CreateCommand(connection);

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
            string.Join(',', values.Select((v, i) => FormatParameterName(prefix, i, "v")))
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
        string FormatParameterNameWithType(string prefix, int i, string type) => FormatParameterName(prefix, i, type);

        var prefix = GetNextId();
        sb.Append(
            string.Join(" AND ",
                conditions.Select((c, i)
                    => $"{Quote(c.Column.Name)} {GetOperationString(c.Operation)} {FormatParameterNameWithType(prefix, i, "cv")}")
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
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
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
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCommand(connection, $"PRAGMA table_info('{_table[^1]}');");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var columns = new List<Column>();
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(new Column(
                reader.GetString("name"),
                GetQueryCatDataType(reader.GetString("type")))
            );
        }
        return columns.ToArray();
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
        var id = identifiers[0];
        if (id.StartsWith('\"'))
        {
            return id;
        }

        return $"\"{id.Replace("\"", "\"\"")}\"";
    }

    private static string GetDatabaseDataType(DataType type)
        => type switch
        {
            DataType.Integer => "INTEGER",
            DataType.Numeric => "NUMERIC",
            DataType.String => "TEXT",
            DataType.Timestamp => "TEXT",
            DataType.Float => "REAL",
            DataType.Boolean => "INTEGER",
            DataType.Blob => "BLOB",
            _ => "TEXT",
        };

    private static DataType GetQueryCatDataType(string type)
        => type switch
        {
            "INTEGER" => DataType.Integer,
            "TEXT" => DataType.String,
            "REAL" => DataType.Float,
            "NUMERIC" => DataType.Numeric,
            "BLOB" => DataType.Blob,
            _ => DataType.String,
        };
}
