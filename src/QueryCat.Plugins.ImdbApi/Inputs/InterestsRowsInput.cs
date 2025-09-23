using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestSharp;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.ImdbApi.Models;
using QueryCat.Plugins.ImdbApi.Utils;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal sealed class InterestsRowsInput : AsyncEnumerableRowsInput<InterestModel>
{
    [SafeFunction]
    [Description("IMDb interesnts table.")]
    [FunctionSignature("imdb_interest(): object<IRowsIterator>")]
    public static VariantValue ImdbInterestsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new InterestsRowsInput());
    }

    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(InterestsRowsInput));

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<InterestModel> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.Category)
            .AddProperty(p => p.Name)
            .AddProperty(p => p.Description)
            .AddProperty(p => p.IsSubgenre);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<InterestModel> GetDataAsync(Fetcher<InterestModel> fetcher, CancellationToken cancellationToken = default)
    {
        var currentPageToken = string.Empty;

        return fetcher.FetchUntilHasMoreAsync(
            async ct =>
            {
                var request = new RestRequest("interests")
                    .AddParameter("pageToken", currentPageToken);

                _logger.LogDebug("Request: {Request}.", request.Dump(ImdbConnection.Client));
                var response = await ImdbConnection.Client.GetAsync(request, ct);
                var node = JsonSerializer.Deserialize(response.Content ?? "{}", SourceGenerationContext.Default.JsonElement);
                if (!node.TryGetProperty("categories", out var categoriesNode))
                {
                    return ([], false);
                }
                var result = categoriesNode.Deserialize(SourceGenerationContext.Default.IListInterestCategoryModel) ?? [];

                foreach (var item in result)
                {
                    foreach (var interest in item.Interests)
                    {
                        interest.Category = item.Category;
                    }
                }

                currentPageToken = ImdbConnection.GetNextPageToken(node);
                return (result.SelectMany(c => c.Interests), currentPageToken.Length > 0);
            }, cancellationToken);
    }
}
