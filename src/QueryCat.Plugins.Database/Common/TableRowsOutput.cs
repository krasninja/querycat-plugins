using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

internal abstract class TableRowsOutput : IRowsOutput, IDisposable
{
    private readonly TableRowsProvider _provider;
    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(TableRowsOutput));

    public string TableName { get; protected set; }

    protected string[] KeyColumnsNames { get; }

    private int[] _keysColumnsIndexes = [];
    private Column[] _keyColumns = [];
    private readonly bool _skipUpdateIfExists;

    private bool _isOpened;

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    /// <inheritdoc />
    public RowsOutputOptions Options { get; } = new();

    public TableRowsOutput(TableRowsProvider provider, bool skipUpdateIfExists, string tableName, string? keys = null)
    {
        _provider = provider;
        _skipUpdateIfExists = skipUpdateIfExists;
        TableName = tableName;
        KeyColumnsNames = (keys ?? string.Empty).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    /// <inheritdoc />
    public virtual async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_isOpened)
        {
            return;
        }

        await _provider.OpenAsync(cancellationToken);

        // Create the initial table with primary key.
        await _provider.CreateDatabaseTableAsync(cancellationToken);
        var columns = QueryContext.QueryInfo.Columns.ToArray();

        // Initialize _keysColumnsIndexes with keys columns.
        _keysColumnsIndexes = new int[KeyColumnsNames.Length];
        _keyColumns = new Column[KeyColumnsNames.Length];
        for (var i = 0; i < KeyColumnsNames.Length; i++)
        {
            var index = Array.FindIndex(columns, c => c.Name == KeyColumnsNames[i]);
            if (index < 0)
            {
                throw new QueryCatException($"Cannot find key column '{KeyColumnsNames[i]}'.");
            }
            _keysColumnsIndexes[i] = index;
            _keyColumns[i] = columns[index];
        }

        // Add missing columns to the table.
        var existingColumns = (await _provider.GetDatabaseTableColumnsAsync(cancellationToken))
            .Select(c => c.Name)
            .ToHashSet();
        foreach (var column in QueryContext.QueryInfo.Columns)
        {
            if (!existingColumns.Contains(column.Name))
            {
                await _provider.CreateDatabaseColumnAsync(column, cancellationToken);
                _logger.LogDebug("Create column {Column}.", column.ToString());
            }
        }

        // Create unique index on keys columns.
        if (KeyColumnsNames.Any())
        {
            await _provider.CreateDatabaseUniqueKeysIndexAsync(_keyColumns, cancellationToken);
        }

        _isOpened = true;
    }

    /// <inheritdoc />
    public virtual Task CloseAsync(CancellationToken cancellationToken = default)
    {
        _isOpened = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task ResetAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<ErrorCode> WriteValuesAsync(VariantValue[] values, CancellationToken cancellationToken = default)
    {
        if (!_isOpened)
        {
            return ErrorCode.Closed;
        }

        var keys = GetKeysValues(values);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            if (keys.Length > 0)
            {
                _logger.LogTrace("Start insert record with keys '{Keys}'.",
                    string.Join(',', keys));
            }
            else
            {
                _logger.LogTrace("Start insert record.");
            }
        }
        var modification = new TableRowsModification(
            columns: QueryContext.QueryInfo.Columns.ToArray(),
            keyColumns: _keyColumns,
            values: values,
            keys: keys);
        var ids = await _provider.InsertDatabaseRowsAsync([modification], cancellationToken);
        if (ids.Length < 1)
        {
            if (!_skipUpdateIfExists)
            {
                await _provider.UpdateDatabaseRowAsync([modification], cancellationToken);
                _logger.LogTrace("Updated record with keys '{Keys}'.", string.Join(',', keys));
            }
            else
            {
                _logger.LogTrace("Skip record update.");
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Inserted record with id {Id} and keys '{Keys}'.",
                    string.Join(',', ids),
                    string.Join(',', keys));
            }
        }

        return ErrorCode.OK;
    }

    private VariantValue[] GetKeysValues(in VariantValue[] values)
    {
        var keys = new VariantValue[_keysColumnsIndexes.Length];
        for (var i = 0; i < _keysColumnsIndexes.Length; i++)
        {
            keys[i] = values[_keysColumnsIndexes[i]];
        }
        return keys;
    }

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            (_provider as IDisposable)?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
