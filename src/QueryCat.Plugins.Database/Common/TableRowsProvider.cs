using System.Data.Common;
using System.Text;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

/// <summary>
/// Queries execution implementation for the specific database provider.
/// </summary>
internal abstract class TableRowsProvider
{
    public const string IdentityColumnName = "_id";
    public static readonly Column IdentityColumn = new(IdentityColumnName, DataType.Integer, "Primary key identity.");

    private int _currentId;

    /// <summary>
    /// Get the next id.
    /// </summary>
    /// <returns></returns>
    protected string GetNextId() => (++_currentId).ToString();

    /// <summary>
    /// Open provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public abstract Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create database command.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="query">SQL.</param>
    /// <returns>Instance of <see cref="DbCommand" />.</returns>
    public virtual DbCommand CreateCommand(DbDataSource dataSource, string? query = null)
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

    /// <summary>
    /// Create the reader that selects all columns from the table.
    /// </summary>
    /// <param name="selectColumns">Column to select.</param>
    /// <param name="conditions">Conditions to select.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Instance of <see cref="DbDataReader" />.</returns>
    public abstract Task<DbDataReader> CreateDatabaseSelectReaderAsync(
        Column[] selectColumns,
        IReadOnlyList<TableSelectCondition> conditions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete the current row from the table.
    /// </summary>
    /// <param name="id">Identifier of the current record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public abstract ValueTask DeleteDatabaseRowAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create database table.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public abstract Task CreateDatabaseTableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create the index on keys columns.
    /// </summary>
    /// <param name="keyColumns">Key columns.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public abstract Task CreateDatabaseUniqueKeysIndexAsync(Column[] keyColumns, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add column to database table.
    /// </summary>
    /// <param name="column">Column instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public abstract Task CreateDatabaseColumnAsync(Column column, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert the record into database and return its identifier. If the record already exists
    /// return 0.
    /// </summary>
    /// <param name="data">Modification data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Identifier of the new records or empty if exists.</returns>
    public abstract ValueTask<long[]> InsertDatabaseRowsAsync(
        TableRowsModification[] data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the database row by keys.
    /// </summary>
    /// <param name="data">Modification data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public abstract ValueTask UpdateDatabaseRowAsync(
        TableRowsModification[] data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current table columns.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Columns list.</returns>
    public abstract Task<Column[]> GetDatabaseTableColumnsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Quote the identifiers. For example, [public, Table] -> "public"."Table".
    /// </summary>
    /// <param name="identifiers">Identifiers to quote.</param>
    /// <returns>Quoted.</returns>
    protected abstract string Quote(params ReadOnlySpan<string> identifiers);

    /// <summary>
    /// Adds block: "column1", "column2".
    /// </summary>
    protected virtual void AppendSelectColumnsBlock(StringBuilder sb, Column[] selectColumns)
    {
        if (selectColumns.Length == 0)
        {
            sb.Append("*");
        }
        else
        {
            sb.Append(string.Join(", ", selectColumns.Select(c => Quote(c.Name))));
        }
    }

    /// <summary>
    /// Adds block: @v1, @v2.
    /// </summary>
    protected virtual void AppendSelectValuesBlock(StringBuilder sb, DbCommand command, params VariantValue[] values)
    {
        var prefix = GetNextId();
        sb.Append(
            string.Join(',', values.Select((_, i) => FormatParameterName(prefix, i, "v")))
        );
        for (var i = 0; i < values.Length; i++)
        {
            var param = command.CreateParameter();
            param.ParameterName = FormatParameterNamePlain(prefix, i, "v");
            param.Value = NormalizeValue(values[i]);
            command.Parameters.Add(param);
        }
    }

    /// <summary>
    /// Adds block: "column1" >= 10 AND "column2" = 'test'.
    /// </summary>
    protected virtual void AppendWhereConditionsBlock(StringBuilder sb, DbCommand command,
        params IReadOnlyList<TableSelectCondition> conditions)
    {
        var prefix = GetNextId();
        sb.Append(
            string.Join(" AND ",
                conditions.Select((c, i)
                    => $"{Quote(c.Column.Name)} {GetOperationString(c.Operation)} {FormatParameterName(prefix, i, "cv")}")
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

    /// <summary>
    /// Adds block: "column1" = 10 AND "column2" = 'test'.
    /// </summary>
    protected void AppendWhereConditionsBlock(StringBuilder sb, DbCommand command, TableRowsModification data)
    {
        var conditions = data.GetKeyValues()
            .Select(kv => new TableSelectCondition(kv.Key, kv.Value, VariantValue.Operation.Equals))
            .ToArray();
        AppendWhereConditionsBlock(sb, command, conditions);
    }

    /// <summary>
    /// Adds block: "column1" = 'val1', "column2" = 2.
    /// </summary>
    protected virtual void AppendUpdateValuesBlock(StringBuilder sb, DbCommand command, TableRowsModification data)
    {
        var prefix = GetNextId();
        sb.Append(
            string.Join(',', data.Columns.Select((c, i) =>
                $"{Quote(c.Name)} = {FormatParameterName(prefix, i)}"))
            );
        for (var i = 0; i < data.Values.Length; i++)
        {
            var param = command.CreateParameter();
            param.ParameterName = FormatParameterNamePlain(prefix, i);
            param.Value = NormalizeValue(data.Values[i]);
            command.Parameters.Add(param);
        }
    }

    protected virtual async ValueTask<long[]> ExecuteScalarsAsync(DbCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!reader.HasRows || reader.FieldCount < 1)
        {
            return [];
        }

        var ids = new List<long>();
        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetInt64(0));
        }
        return ids.ToArray();
    }

    /// <summary>
    /// Adds additional formatting to SQL parameter.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <returns>Final name.</returns>
    protected virtual string FormatParameterName(string name) => name;

    /// <summary>
    /// Adds additional formatting to SQL parameter. For example, "@p1_1".
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <param name="index">Current parameter number.</param>
    /// <param name="type">Parameter type.</param>
    /// <returns>Final name.</returns>
    protected string FormatParameterName(string prefix, int index, string type = "p")
        => FormatParameterName(FormatParameterNamePlain(prefix, index, type));

    /// <summary>
    /// Format parameter name without prefix character. For example, "p1_1".
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <param name="index">Current parameter number.</param>
    /// <param name="type">Parameter type.</param>
    /// <returns>Final name.</returns>
    protected string FormatParameterNamePlain(string prefix, int index, string type = "p")
        => string.Concat(type, prefix, "_", index);

    protected virtual string GetOperationString(VariantValue.Operation operation) => operation switch
    {
        VariantValue.Operation.Add => "+",
        VariantValue.Operation.Subtract => "-",
        VariantValue.Operation.Multiple => "*",
        VariantValue.Operation.Divide => "/",
        VariantValue.Operation.Modulo => "%",
        VariantValue.Operation.LeftShift => "<<",
        VariantValue.Operation.RightShift => ">>",

        VariantValue.Operation.Equals => "=",
        VariantValue.Operation.NotEquals => "!=",
        VariantValue.Operation.Greater => ">",
        VariantValue.Operation.GreaterOrEquals => ">=",
        VariantValue.Operation.Less => "<",
        VariantValue.Operation.LessOrEquals => "<=",
        VariantValue.Operation.Between => "BETWEEN",
        VariantValue.Operation.BetweenAnd => string.Empty,
        VariantValue.Operation.IsNull => "IS NULL",
        VariantValue.Operation.IsNotNull => "IS NOT NULL",
        VariantValue.Operation.Like => "LIKE",
        VariantValue.Operation.NotLike => "NOT LIKE",
        VariantValue.Operation.Similar => "SIMILAR",
        VariantValue.Operation.NotSimilar => "NOT SIMILAR",
        VariantValue.Operation.In => "IN",

        VariantValue.Operation.And => "AND",
        VariantValue.Operation.Or => "OR",
        VariantValue.Operation.Not => "NOT",

        VariantValue.Operation.Concat => "||",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };

    public static object NormalizeValue(in VariantValue value)
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
}
