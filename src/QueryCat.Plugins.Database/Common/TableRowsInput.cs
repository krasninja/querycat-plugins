using System.Data;
using System.Data.Common;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

internal abstract class TableRowsInput : IRowsInputDelete, IRowsInputKeys, IDisposable
{
    private readonly TableRowsProvider _provider;

    /// <inheritdoc />
    public Column[] Columns { get; protected set; } = [];

    public string TableName { get; }

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    private DbDataReader? _reader;

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
    public ErrorCode DeleteCurrent()
    {
        if (_reader == null)
        {
            return ErrorCode.Closed;
        }
        var id = _reader.GetInt64(0);
        _provider.DeleteDatabaseRow(id);
        return ErrorCode.OK;
    }

    /// <inheritdoc />
    public virtual void Open()
    {
        if (_reader != null)
        {
            _reader.Close();
            _reader = null;
        }

        _reader = _provider.CreateDatabaseSelectReader(QueryContext.QueryInfo.Columns.ToArray());

        var dbColumns = _reader.GetColumnSchema();
        var columns = new List<Column>();
        foreach (var dbColumn in dbColumns)
        {
            if (dbColumn.DataType == null)
            {
                continue;
            }
            columns.Add(new Column(dbColumn.ColumnName, ConvertFromSystem(dbColumn.DataType)));
        }
        Columns = columns.ToArray();
    }

    /// <inheritdoc />
    public virtual void Close()
    {
        _reader?.Close();
        _reader = null;
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
        Close();
        Open();
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
    public virtual bool ReadNext()
    {
        if (_reader == null)
        {
            return false;
        }
        return _reader.Read();
    }

    /// <inheritdoc />
    public IReadOnlyList<KeyColumn> GetKeyColumns() => Columns.Select((c, i) => new KeyColumn(i)).ToArray();

    /// <inheritdoc />
    public void SetKeyColumnValue(int columnIndex, VariantValue value, VariantValue.Operation operation)
    {
    }

    /// <inheritdoc />
    public void Explain(IndentedStringBuilder stringBuilder)
    {
    }

    private static DataType ConvertFromSystem(Type type)
    {
        var result = Converter.ConvertFromSystem(type);
        if (result == DataType.Void)
        {
            return DataType.String;
        }
        return result;
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
