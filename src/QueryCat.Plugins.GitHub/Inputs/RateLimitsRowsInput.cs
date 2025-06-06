using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

internal sealed class RateLimitsRowsInput : BaseRowsInput<MiscellaneousRateLimit>
{
    [SafeFunction]
    [Description("Return GitHub current account rate limit information.")]
    [FunctionSignature("github_rate_limits(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubRateLimitsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new RateLimitsRowsInput(token));
    }

    public RateLimitsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<MiscellaneousRateLimit> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("limit", p => p.Rate.Limit, "The maximum number of requests that the consumer is permitted to make per hour.")
            .AddProperty("remaining", p => p.Rate.Remaining, "The number of requests remaining in the current rate limit window.")
            .AddProperty("reset", p => p.Rate.Reset, "The date and time at which the current rate limit window resets.")
            .AddProperty("reset_at_utc_epoch", p => p.Rate.ResetAsUtcEpochSeconds,
                "The date and time at which the current rate limit window resets - in UTC epoch seconds.");
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<MiscellaneousRateLimit> GetDataAsync(
        Fetcher<MiscellaneousRateLimit> fetcher,
        CancellationToken cancellationToken = default)
    {
        return fetcher.FetchOneAsync(
            async ct => await Client.RateLimit.GetRateLimits(), cancellationToken);
    }
}
