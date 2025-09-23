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

internal sealed class TitlesRowsInput : AsyncEnumerableRowsInput<TitleModel>
{
    [SafeFunction]
    [Description("IMDb titles table.")]
    [FunctionSignature("imdb_title(): object<IRowsIterator>")]
    public static VariantValue ImdbTitlesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new TitlesRowsInput());
    }

    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(TitlesRowsInput));

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<TitleModel> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
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
            .AddProperty(p => p.Plot)
            .AddKeyColumn(p => p.PrimaryTitle, VariantValue.Operation.Like)
            .AddKeyColumn(p => p.PrimaryTitle, VariantValue.Operation.Equals)
            .AddKeyColumn(p => p.Id)
            .AddKeyColumn(p => p.Type)
            .AddKeyColumn(p => p.StartYear)
            .AddKeyColumn(p => p.EndYear)
            .AddKeyColumn(p => p.Rating.VoteCount, VariantValue.Operation.GreaterOrEquals)
            .AddKeyColumn(p => p.Rating.VoteCount, VariantValue.Operation.LessOrEquals)
            .AddKeyColumn(p => p.Rating.AggregateRating, VariantValue.Operation.GreaterOrEquals)
            .AddKeyColumn(p => p.Rating.AggregateRating, VariantValue.Operation.LessOrEquals);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TitleModel> GetDataAsync(Fetcher<TitleModel> fetcher, CancellationToken cancellationToken = default)
    {
        var currentPageToken = string.Empty;

        return fetcher.FetchUntilHasMoreAsync(
            async ct =>
            {
                var request = new RestRequest("titles")
                    .AddParameter("pageToken", currentPageToken);

                // Select by id case.
                if (TryGetKeyColumnValue("id", VariantValue.Operation.Equals, out var idValue))
                {
                    request = new RestRequest("titles/{titleId}")
                        .AddUrlSegment("titleId", idValue.AsString);
                    _logger.LogDebug("Request: {Request}.", request.Dump(ImdbConnection.Client));
                    var idResponse = await ImdbConnection.Client.GetAsync<TitleModel>(request, ct);
                    return (idResponse != null ? [idResponse] : [], false);
                }

                // Search
                if (TryGetKeyColumnValue("primary_title", VariantValue.Operation.Equals, out var titleValue)
                    || TryGetKeyColumnValue("primary_title", VariantValue.Operation.Like, out titleValue))
                {
                    var titleToSearch = titleValue.AsString
                        .Replace("%", string.Empty).Replace("?", string.Empty);
                    request = new RestRequest("search/titles")
                        .AddParameter("query", titleToSearch)
                        .AddParameter("pageToken", currentPageToken);
                    _logger.LogDebug("Request: {Request}.", request.Dump(ImdbConnection.Client));
                    var searchResponse = await ImdbConnection.Client.GetAsync(request, ct);
                    var searchNode = JsonSerializer.Deserialize(searchResponse.Content ?? "{}", SourceGenerationContext.Default.JsonElement);
                    var searchResult = searchNode.GetProperty("titles").Deserialize(SourceGenerationContext.Default.IListTitleModel);
                    currentPageToken = ImdbConnection.GetNextPageToken(searchNode);
                    return (searchResult ?? [], currentPageToken.Length > 0);
                }

                if (TryGetKeyColumnValue("type", VariantValue.Operation.Equals, out var typeValue))
                {
                    request.AddParameter("type", typeValue.AsString);
                }
                if (TryGetKeyColumnValue("start_year", VariantValue.Operation.Equals, out var startYearValue))
                {
                    request.AddParameter("startYear", startYearValue.AsString);
                }
                if (TryGetKeyColumnValue("end_year", VariantValue.Operation.Equals, out var endYearValue))
                {
                    request.AddParameter("endYear", endYearValue.AsString);
                }
                if (TryGetKeyColumnValue("vote_count", VariantValue.Operation.GreaterOrEquals, out var voteGteValue))
                {
                    request.AddParameter("minVoteCount", voteGteValue.AsString);
                }
                if (TryGetKeyColumnValue("vote_count", VariantValue.Operation.LessOrEquals, out var voteLteValue))
                {
                    request.AddParameter("maxVoteCount", voteLteValue.AsString);
                }
                if (TryGetKeyColumnValue("aggregate_rating", VariantValue.Operation.GreaterOrEquals, out var ratingGteValue))
                {
                    request.AddParameter("minAggregateRating", ratingGteValue.AsString);
                }
                if (TryGetKeyColumnValue("aggregate_rating", VariantValue.Operation.LessOrEquals, out var ratingLteValue))
                {
                    request.AddParameter("maxAggregateRating", ratingLteValue.AsString);
                }

                _logger.LogDebug("Request: {Request}.", request.Dump(ImdbConnection.Client));
                var response = await ImdbConnection.Client.GetAsync(request, ct);
                var node = JsonSerializer.Deserialize(response.Content ?? "{}", SourceGenerationContext.Default.JsonElement);
                if (!node.TryGetProperty("titles", out var titlesNode))
                {
                    return ([], false);
                }
                currentPageToken = ImdbConnection.GetNextPageToken(node);
                var result = titlesNode.Deserialize(SourceGenerationContext.Default.IListTitleModel) ?? [];
                return (result, currentPageToken.Length > 0);
            }, cancellationToken);
    }
}
