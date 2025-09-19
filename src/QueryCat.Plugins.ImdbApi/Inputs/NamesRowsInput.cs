using System.ComponentModel;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.ImdbApi.Models;
using RestSharp;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal sealed class NamesRowsInput : AsyncEnumerableRowsInput<NameModel>
{
    [SafeFunction]
    [Description("IMDb names table.")]
    [FunctionSignature("imdb_name(): object<IRowsIterator>")]
    public static VariantValue ImdbNamesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new NamesRowsInput());
    }

    private readonly ILogger _logger = Application.LoggerFactory.CreateLogger(nameof(NamesRowsInput));
    private VariantValue _nameId = VariantValue.Null;

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<NameModel> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.DisplayName)
            .AddProperty(p => p.Biography)
            .AddProperty("birth_date", p => p.BirthDate.ToDate(), "The birth name of the person, which may differ from their display name.")
            .AddProperty("death_date", p => p.DeathDate.ToDate(), "The death date of the person.")
            .AddProperty(p => p.DeathLocation)
            .AddProperty(p => p.DeathReason)
            .AddProperty(p => p.HeightCm)
            .AddKeyColumn(p => p.Id, isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<NameModel> GetDataAsync(Fetcher<NameModel> fetcher, CancellationToken cancellationToken = default)
    {
        _nameId = GetKeyColumnValue("id");

        return fetcher.FetchUntilHasMoreAsync(
            async ct =>
            {
                var request = new RestRequest($"names/{_nameId.AsString}");

                _logger.LogDebug("Get name {NameId}.", _nameId);
                var response = await ImdbConnection.Client.GetAsync<NameModel>(request, ct);
                return ([response!], false);
            }, cancellationToken);
    }
}
