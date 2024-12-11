using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

/// <summary>
/// Describes the modification (insert or update) request.
/// </summary>
public sealed class TableRowsModification
{
    public Column[] Columns { get; }

    public Column[] KeyColumns { get; }

    public VariantValue[] Keys { get; }

    public VariantValue[] Values { get; }

    public TableRowsModification(Column[] columns, Column[] keyColumns, VariantValue[] keys, VariantValue[] values)
    {
        Columns = columns;
        KeyColumns = keyColumns;
        Keys = keys;
        Values = values;
    }

    public IEnumerable<KeyValuePair<Column, VariantValue>> GetKeyValues()
    {
        for (var i = 0; i < KeyColumns.Length; i++)
        {
            yield return new KeyValuePair<Column, VariantValue>(KeyColumns[i], Keys[i]);
        }
    }

    public IEnumerable<KeyValuePair<Column, VariantValue>> GetColumnValues()
    {
        for (var i = 0; i < Columns.Length; i++)
        {
            yield return new KeyValuePair<Column, VariantValue>(Columns[i], Values[i]);
        }
    }
}
