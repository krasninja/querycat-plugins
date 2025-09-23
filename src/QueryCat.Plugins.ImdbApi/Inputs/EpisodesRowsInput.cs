using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.ImdbApi.Models;
using QueryCat.Plugins.ImdbApi.Utils;
using RestSharp;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal sealed class EpisodesRowsInput : AsyncEnumerableRowsInput<EpisodeModel>
{
    [SafeFunction]
    [Description("IMDb episodes table.")]
    [FunctionSignature("imdb_episode(): object<IRowsIterator>")]
    public static VariantValue ImdbEpisodesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new EpisodesRowsInput());
    }

    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(EpisodesRowsInput));
    private VariantValue _titleId = new(string.Empty);

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<EpisodeModel> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.Title)
            .AddProperty(p => p.Season)
            .AddProperty("title_id", DataType.String, _ => _titleId, "The ID of the parent title (series) of the episode.")
            .AddProperty(p => p.EpisodeNumber)
            .AddProperty(p => p.RuntimeSeconds)
            .AddProperty("date", p => p.Date.ToDate(), "The release date of the episode.")
            .AddProperty(p => p.Rating.AggregateRating)
            .AddProperty(p => p.Rating.VoteCount)
            .AddProperty(p => p.Plot)
            .AddKeyColumn("title_id", isRequired: true)
            .AddKeyColumn(p => p.Season);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<EpisodeModel> GetDataAsync(Fetcher<EpisodeModel> fetcher, CancellationToken cancellationToken = default)
    {
        var currentPageToken = string.Empty;
        _titleId = GetKeyColumnValue("title_id");

        return fetcher.FetchUntilHasMoreAsync(
            async ct =>
            {
                var request = new RestRequest("titles/{titleId}/episodes")
                    .AddUrlSegment("titleId", _titleId.AsString)
                    .AddParameter("pageToken", currentPageToken);

                if (TryGetKeyColumnValue("season", VariantValue.Operation.Equals, out var typeValue)
                    && !typeValue.IsNull)
                {
                    request.AddParameter("season", typeValue.AsString);
                }

                _logger.LogDebug("Request: {Request}.", request.Dump(ImdbConnection.Client));
                var response = await ImdbConnection.Client.GetAsync(request, ct);
                var node = JsonSerializer.Deserialize(response.Content ?? "{}", SourceGenerationContext.Default.JsonElement);
                if (!node.TryGetProperty("episodes", out var episodesNode))
                {
                    return ([], false);
                }
                currentPageToken = ImdbConnection.GetNextPageToken(node);
                var result = episodesNode.Deserialize(SourceGenerationContext.Default.IListEpisodeModel) ?? [];
                return (result, currentPageToken.Length > 0);
            }, cancellationToken);
    }
}
