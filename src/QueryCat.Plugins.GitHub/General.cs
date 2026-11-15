using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;

namespace QueryCat.Plugins.Github;

/// <summary>
/// GitHub functions.
/// </summary>
internal static class General
{
    public const string AuthProviderId = "github";
    public const string GitHubToken = "github-access-token";

    public static async ValueTask<string> GetTokenAsync(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var authToken = await thread.AuthTokensManager.GetTokenOrDefaultAsync(
            AuthProviderId, cancellationToken: cancellationToken);
        if (authToken != null)
        {
            return authToken.AccessToken;
        }
        var token = await thread.ConfigStorage.GetOrDefaultAsync(GitHubToken, cancellationToken: cancellationToken);
        return token.AsString;
    }
}
