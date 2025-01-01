using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Common;

public record struct TableSelectCondition(
    Column Column,
    VariantValue Value,
    VariantValue.Operation Operation = VariantValue.Operation.Equals);
