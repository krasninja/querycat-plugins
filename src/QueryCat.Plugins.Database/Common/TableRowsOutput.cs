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
    public virtual void Open()
    {
        if (_isOpened)
        {
            return;
        }

        // Create the initial table with primary key.
        _provider.CreateDatabaseTable();
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
        var existingColumns = _provider.GetDatabaseTableColumns().Select(c => c.Name).ToHashSet();
        foreach (var column in QueryContext.QueryInfo.Columns)
        {
            if (!existingColumns.Contains(column.Name))
            {
                _provider.CreateDatabaseColumn(column);
            }
        }

        // Create unique index on keys columns.
        if (KeyColumnsNames.Any())
        {
            _provider.CreateDatabaseUniqueKeysIndex(_keyColumns);
        }

        _isOpened = true;
    }

    /// <inheritdoc />
    public virtual void Close()
    {
        _isOpened = false;
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
    }

    /// <inheritdoc />
    public ErrorCode WriteValues(VariantValue[] values)
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
        var id = _provider.InsertDatabaseRow(modification);
        if (id < 1 && keys.Any())
        {
            _provider.UpdateDatabaseRow(modification);
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
