using System.Data.Common;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

/// <summary>
/// Queries execution implementation for the specific database provider.
/// </summary>
internal abstract class TableRowsProvider
{
    public const string IdentityColumnName = "_id";

    /// <summary>
    /// Create the reader that selects all columns from the table.
    /// </summary>
    /// <param name="columns">Column to select.</param>
    /// <returns>Instance of <see cref="DbDataReader" />.</returns>
    public abstract DbDataReader CreateDatabaseSelectReader(Column[] columns);

    /// <summary>
    /// Delete the current row from the table.
    /// </summary>
    /// <param name="id">Identifier of the current record.</param>
    public abstract void DeleteDatabaseRow(long id);

    /// <summary>
    /// Create database table.
    /// </summary>
    public abstract void CreateDatabaseTable();

    /// <summary>
    /// Create the index on keys columns.
    /// </summary>
    /// <param name="keyColumns">Key columns.</param>
    public abstract void CreateDatabaseUniqueKeysIndex(Column[] keyColumns);

    /// <summary>
    /// Add column to database table.
    /// </summary>
    /// <param name="column">Column instance.</param>
    public abstract void CreateDatabaseColumn(Column column);

    /// <summary>
    /// Insert the record into database and return its identifier. If the record already exists
    /// return 0.
    /// </summary>
    /// <param name="data">Modification data.</param>
    /// <returns>Identifier of the new record or zero if exists.</returns>
    public abstract long InsertDatabaseRow(TableRowsModification data);

    /// <summary>
    /// Update the database row by keys.
    /// </summary>
    /// <param name="data">Modification data.</param>
    public abstract void UpdateDatabaseRow(TableRowsModification data);

    /// <summary>
    /// Get current table columns.
    /// </summary>
    /// <returns>Columns list.</returns>
    public abstract Column[] GetDatabaseTableColumns();

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
