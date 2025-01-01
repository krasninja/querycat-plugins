using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.VisualParadigm.Models;

namespace QueryCat.Plugins.VisualParadigm.Inputs;

internal sealed class DiagramElementsRowsInput : AsyncEnumerableRowsInput<DiagramElement>
{
    [SafeFunction]
    [Description("Get diagram elements.")]
    [FunctionSignature("vp_diagram_elements(db: string): object<IRowsInput>")]
    public static VariantValue DiagramElementsInput(IExecutionThread thread)
    {
        var db = thread.Stack.Pop();
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
    protected override async IAsyncEnumerable<DiagramElement> GetDataAsync(Fetcher<DiagramElement> fetcher,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_db}");
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM MODEL_ELEMENT";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new DiagramElement
            {
                DiagramId = "test"
            };
        }
    }
}
