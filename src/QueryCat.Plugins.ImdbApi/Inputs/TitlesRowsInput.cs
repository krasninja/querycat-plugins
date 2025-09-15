using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.ImdbApi.Models;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal class TitlesRowsInput : AsyncEnumerableRowsInput<TitleModel>
{
    [SafeFunction]
    [Description("Imdb titles table.")]
    [FunctionSignature("imdb_title(): object<IRowsIterator>")]
    public static VariantValue ImdbTitlesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new TitlesRowsInput());
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<TitleModel> builder)
    {
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.Type)
            .AddProperty(p => p.IsAdult)
            .AddProperty(p => p.OriginalTitle)
            .AddProperty(p => p.PrimaryTitle)
            .AddProperty(p => p.RuntimeSeconds)
            .AddProperty(p => p.StartYear)
            .AddProperty(p => p.EndYear)
            .AddProperty(p => p.Rating.AggregateRating)
            .AddProperty(p => p.Rating.VoteCount)
            .AddKeyColumn("type");
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TitleModel> GetDataAsync(Fetcher<TitleModel> fetcher, CancellationToken cancellationToken = default)
    {
        var nextPageToken = string.Empty;
        return null;
    }
}
