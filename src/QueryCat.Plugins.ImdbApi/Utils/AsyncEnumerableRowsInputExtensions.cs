using System.Text;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.ImdbApi.Utils;

/// <summary>
/// Extensions for <see cref="AsyncEnumerableRowsInputExtensions" />.
/// </summary>
internal static class AsyncEnumerableRowsInputExtensions
{
    public static string DumpInputKeys<T>(this AsyncEnumerableRowsInput<T> input) where T : class
    {
        var sb = new StringBuilder();
        var isFirst = true;
        foreach (var keyColumn in input.GetKeyColumns())
        {
            foreach (var operation in keyColumn.GetOperations())
            {
                var column = input.Columns[keyColumn.ColumnIndex];
                if (input.TryGetKeyColumnValue(column.Name, operation, out var value))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(',');
                    }

                    sb.Append(column.Name)
                        .Append(GetOperationString(operation))
                        .Append(value);
                }
            }
        }

        return sb.ToString();
    }

    private static string GetOperationString(VariantValue.Operation operation) => operation switch
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
}
