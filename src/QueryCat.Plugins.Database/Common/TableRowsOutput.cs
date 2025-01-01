using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

internal abstract class TableRowsOutput : IRowsOutput, IDisposable
{
    private readonly TableRowsProvider _provider;

    public string TableName { get; protected set; }

    protected string[] KeyColumnsNames { get; }

    private int[] _keysColumnsIndexes = [];
    private Column[] _keyColumns = [];

    private bool _isOpened;

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    /// <inheritdoc />
    public RowsOutputOptions Options { get; } = new();

    public TableRowsOutput(TableRowsProvider provider, string tableName, string? keys = null)
    {
        _provider = provider;
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
            .Select(c => c.Name).ToHashSet();
        foreach (var column in QueryContext.QueryInfo.Columns)
        {
            if (!existingColumns.Contains(column.Name))
            {
                await _provider.CreateDatabaseColumnAsync(column, cancellationToken);
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
        var modification = new TableRowsModification(
            columns: QueryContext.QueryInfo.Columns.ToArray(),
            keyColumns: _keyColumns,
            values: values,
            keys: keys);
        var ids = await _provider.InsertDatabaseRowsAsync([modification], cancellationToken);
        if (ids.Length < 1)
        {
            await _provider.UpdateDatabaseRowAsync([modification], cancellationToken);
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
