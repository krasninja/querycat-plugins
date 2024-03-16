using System.ComponentModel;
using Microsoft.Data.Sqlite;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.VisualParadigm.Models;

namespace QueryCat.Plugins.VisualParadigm.Inputs;

internal sealed class DiagramElementsRowsInput : FetchRowsInput<DiagramElement>
{
    [SafeFunction]
    [Description("Get diagram elements.")]
    [FunctionSignature("vp_diagram_elements(db: string): object<IRowsInput>")]
    public static VariantValue DiagramElementsInput(FunctionCallInfo args)
    {
        var db = args.GetAt(0).AsString;
        return VariantValue.CreateFromObject(new DiagramElementsRowsInput(db));
    }

    private const int Count = 100;
    private readonly string _db;

    public DiagramElementsRowsInput(string db)
    {
        _db = db;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<DiagramElement> builder)
    {
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.DiagramId)
            .AddProperty(p => p.ShapeType)
            .AddProperty(p => p.ModelElementId);
    }

    /// <inheritdoc />
    protected override IEnumerable<DiagramElement> GetData(Fetcher<DiagramElement> fetcher)
    {
        using var connection = new SqliteConnection($"Data Source={_db}");
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM MODEL_ELEMENT";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return new DiagramElement
            {
                DiagramId = "test"
            };
        }
    }
}
