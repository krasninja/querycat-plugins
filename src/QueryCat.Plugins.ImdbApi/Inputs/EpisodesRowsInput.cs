using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.ImdbApi.Models;
using RestSharp;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal sealed class EpisodesRowsInput : AsyncEnumerableRowsInput<EpisodeModel>
{
    [SafeFunction]
    [Description("Imdb episodes table.")]
    [FunctionSignature("imdb_episode(): object<IRowsIterator>")]
    public static VariantValue ImdbEpisodesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new EpisodesRowsInput());
    }

    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(EpisodesRowsInput));
    private VariantValue _titleId = VariantValue.Null;

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<EpisodeModel> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.Title)
            .AddProperty("title_id", _ => _titleId, "The ID of the parent title (series) of the episode.")
            .AddProperty(p => p.EpisodeNumber)
            .AddProperty(p => p.RuntimeSeconds)
            .AddProperty("date", p => p.Date.ToDate(), "The release date of the episode.")
            .AddProperty(p => p.Rating.AggregateRating)
            .AddProperty(p => p.Rating.VoteCount)
            .AddProperty(p => p.Plot)
            .AddKeyColumn(p => p.Id, isRequired: true)
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
                var request = new RestRequest($"titles/{_titleId.AsString}/episodes")
                    .AddParameter("pageToken", currentPageToken);

                if (TryGetKeyColumnValue("season", VariantValue.Operation.Equals, out var typeValue))
                {
                    request.AddParameter("season", typeValue.AsString);
                }

                var args = string.Join('&', request.Parameters.Select(p => p.ToString()));
                _logger.LogDebug("Get title {TitleID} episodes, filter: {Filter}.", _titleId, args);
                var response = await ImdbConnection.Client.GetAsync(request, ct);
                var node = JsonSerializer.Deserialize(response.Content ?? "{}", SourceGenerationContext.Default.JsonElement);
                currentPageToken = node.GetProperty("nextPageToken").GetString();
                var result = node.GetProperty("episodes").Deserialize(SourceGenerationContext.Default.IListEpisodeModel);
                if (result == null || result.Count < 1)
                {
                    return ([], false);
                }
                return (result, true);
            }, cancellationToken);
    }
}
