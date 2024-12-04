using System.Data.Common;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

internal abstract class TableRowsOutput : IRowsOutput, IDisposable
{
    protected const string IdentityColumnName = "_id";

    public string ConnectionString { get; protected set; }

    public string TableName { get; protected set; }

    public string Namespace { get; protected set; }

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    /// <inheritdoc />
    public RowsOutputOptions Options { get; } = new();

    public TableRowsOutput(string connectionString, string tableName, string? @namespace = null)
    {
        ConnectionString = connectionString;
        TableName = tableName;
        Namespace = @namespace ?? string.Empty;
    }

    /// <inheritdoc />
    public virtual void Open()
    {
        var createTableCommand = CreateCreateTableCommand();
        createTableCommand.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public virtual void Close()
    {
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
    }

    /// <inheritdoc />
    public void WriteValues(in VariantValue[] values)
    {
        var insertCommand = CreateInsertCommand();

        for (var i = 0; i < QueryContext.QueryInfo.Columns.Count; i++)
        {
            var parameter = insertCommand.CreateParameter();
            var value = NormalizeValue(values[i]);
            parameter.ParameterName = "p" + i;
            parameter.Value = value;
            insertCommand.Parameters.Add(parameter);
        }

        insertCommand.ExecuteNonQuery();
    }

    private object NormalizeValue(in VariantValue value)
    {
        var v = Converter.ConvertValue(value, typeof(object));
        if (v == null)
        {
            v = DBNull.Value;
        }
        if (v is DateTime dateTimeValue)
        {
            v = dateTimeValue.ToUniversalTime();
        }
        return v;
    }

    protected abstract DbCommand CreateCreateTableCommand();

    protected abstract DbCommand CreateInsertCommand();

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
}
