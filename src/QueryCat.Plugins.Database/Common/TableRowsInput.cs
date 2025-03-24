using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

internal abstract class TableRowsInput : IRowsInputDelete, IRowsInputKeys, IDisposable
{
    private readonly TableRowsProvider _provider;
    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(TableRowsInput));

    /// <inheritdoc />
    public Column[] Columns { get; protected set; } = [];

    public string TableName { get; }

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    private DbDataReader? _reader;
    private readonly Dictionary<(Column, VariantValue.Operation), TableSelectCondition> _keys = new();

    /// <inheritdoc />
    public string[] UniqueKey =>
    [
        TableName,
    ];

    public TableRowsInput(TableRowsProvider provider, string tableName)
    {
        _provider = provider;
        TableName = tableName;
    }

    /// <inheritdoc />
    public async ValueTask<ErrorCode> DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (_reader == null)
        {
            return ErrorCode.Closed;
        }
        var id = _reader.GetInt64(0);
        var removedCount = await _provider.DeleteDatabaseRowAsync(id, cancellationToken);
        return removedCount > 0 ? ErrorCode.OK : ErrorCode.NoData;
    }

    /// <inheritdoc />
    public virtual async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_reader != null)
        {
            await _reader.CloseAsync();
            _reader = null;
        }

        await _provider.OpenAsync(cancellationToken);
        await InitializeColumnsAsync(cancellationToken);

        _logger.LogDebug("Opened.");
    }

    private async Task InitializeColumnsAsync(CancellationToken cancellationToken = default)
    {
        var tableColumns = await _provider.GetDatabaseTableColumnsAsync(cancellationToken);

        // If we have desired columns from user - add them to the list.
        var requestedColumns = QueryContext.QueryInfo.Columns.ToArray();
        if (requestedColumns.Length > 0)
        {
            var targetColumns = new List<Column>(capacity: requestedColumns.Length);

            // Always include "_id" column.
            var idColumn = tableColumns.FirstOrDefault(c => c.Name == TableRowsProvider.IdentityColumnName);
            if (idColumn != null)
            {
                targetColumns.Add(idColumn);
            }

            foreach (var requestedColumn in requestedColumns)
            {
                var tableColumn = Array.Find(tableColumns, c => c.Name == requestedColumn.Name);
                if (tableColumn == null)
                {
                    continue;
                }
                if (targetColumns.All(c => c.Name != requestedColumn.Name))
                {
                    targetColumns.Add(tableColumn);
                }
            }
            Columns = targetColumns.ToArray();
        }

        if (Columns.Length == 0)
        {
            Columns = tableColumns;
        }
    }

    /// <inheritdoc />
    public virtual async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_reader != null)
        {
            await _reader.CloseAsync();
        }
        _reader = null;

        _logger.LogDebug("Closed.");
    }

    /// <inheritdoc />
    public virtual async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await CloseAsync(cancellationToken);
        await OpenAsync(cancellationToken);
    }

    /// <inheritdoc />
    public ErrorCode ReadValue(int columnIndex, out VariantValue value)
    {
        if (_reader == null)
        {
            value = VariantValue.Null;
            return ErrorCode.NoData;
        }
        if (_reader.IsDBNull(columnIndex))
        {
            value = VariantValue.Null;
            return ErrorCode.OK;
        }
        value = VariantValue.CreateFromObject(_reader[columnIndex]);
        return ErrorCode.OK;
    }

    /// <inheritdoc />
    public virtual async ValueTask<bool> ReadNextAsync(CancellationToken cancellationToken = default)
    {
        if (_reader == null)
        {
            _logger.LogDebug("Initialize reader.");
            _reader = await _provider.CreateDatabaseSelectReaderAsync(
                Columns,
                _keys.Select(k => k.Value with { Column = k.Key.Item1 }).ToArray(),
                cancellationToken);
        }
        return await _reader.ReadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public IReadOnlyList<KeyColumn> GetKeyColumns() => Columns
        .Select((c, i) => new KeyColumn(i, isRequired: false,
        [
            VariantValue.Operation.Equals,
            VariantValue.Operation.NotEquals,
            VariantValue.Operation.GreaterOrEquals,
            VariantValue.Operation.Greater,
            VariantValue.Operation.LessOrEquals,
            VariantValue.Operation.Less,
        ]))
        .ToArray();

    /// <inheritdoc />
    public void SetKeyColumnValue(int columnIndex, VariantValue value, VariantValue.Operation operation)
    {
        var column = Columns[columnIndex];
        _keys[(column, operation)] = new TableSelectCondition(column, value, operation);
    }

    /// <inheritdoc />
    public void UnsetKeyColumnValue(int columnIndex, VariantValue.Operation operation)
    {
        var column = Columns[columnIndex];
        _keys.Remove((column, operation));
    }

    /// <inheritdoc />
    public void Explain(IndentedStringBuilder stringBuilder)
    {
    }

    internal static DbType GetDatabaseDataType(DataType type)
        => type switch
        {
            DataType.Integer => DbType.Int64,
            DataType.Numeric => DbType.Decimal,
            DataType.String => DbType.String,
            DataType.Timestamp => DbType.DateTime,
            DataType.Float => DbType.Double,
            DataType.Boolean => DbType.Boolean,
            DataType.Object => DbType.Object,
            DataType.Dynamic => DbType.Object,
            DataType.Interval => DbType.DateTimeOffset,
            DataType.Blob => DbType.Binary,
            _ => DbType.String
        };

    private static DataType GetQueryCatDataType(DbType type)
        => type switch
        {
            DbType.Binary => DataType.Blob,
            DbType.Boolean => DataType.Boolean,
            DbType.Byte => DataType.Integer,
            DbType.SByte => DataType.Integer,
            DbType.Int16 => DataType.Integer,
            DbType.Int32 => DataType.Integer,
            DbType.Int64 => DataType.Integer,
            DbType.UInt16 => DataType.Integer,
            DbType.UInt32 => DataType.Integer,
            DbType.UInt64 => DataType.Integer,
            DbType.Currency => DataType.Numeric,
            DbType.Decimal => DataType.Numeric,
            DbType.Date => DataType.Timestamp,
            DbType.DateTime => DataType.Timestamp,
            DbType.DateTime2 => DataType.Timestamp,
            DbType.DateTimeOffset => DataType.Interval,
            DbType.Object => DataType.Object,
            DbType.Double => DataType.Float,
            DbType.Single => DataType.Float,
            _ => DataType.String,
        };

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _reader?.Dispose();
            (_provider as IDisposable)?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
