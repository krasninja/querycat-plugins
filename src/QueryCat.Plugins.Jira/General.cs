using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Plugins;
using QueryCat.Backend.Core.Plugins.Auth;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira;

/// <summary>
/// Jira functions.
/// </summary>
internal static class General
{
    private const string AccessibleResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";

    private static readonly HttpClient _httpClient = new();

    // Key is "access token|site url", value is Jira Cloud site id.
    private static readonly ConcurrentDictionary<string, string> _cloudIdCache = new();

    public const string AuthProviderId = "jira";

    /// <summary>
    /// Plugin configuration option: Jira instance URL.
    /// </summary>
    public const string UrlOption = "url";

    /// <summary>
    /// Plugin configuration option: username (email) for Jira Cloud API token.
    /// </summary>
    public const string UsernameOption = "username";

    /// <summary>
    /// Create plugin configuration options.
    /// </summary>
    /// <returns>Options.</returns>
    internal static PluginConfigOption[] CreateConfigOptions() =>
    [
        new(UrlOption, DataType.String,
            "Jira instance URL (f.e. https://your-domain.atlassian.net). Required for API key authorization. "
            + "For OAuth2 it selects the site if the account has access to several sites.", new VariantValue("https://jira.atlassian.net")),
        new(UsernameOption, DataType.String,
            "Jira Cloud username (email). Used with API key authorization: the API key is sent using basic authentication. "
            + "If not set, the API key is sent as a bearer token (personal access token for Jira Data Center/Server)."),
    ];

    /// <summary>
    /// Create Jira configuration. The OAuth2 token or API key is provided by the host ("qcat plugin auth login jira").
    /// </summary>
    /// <param name="thread">Execution thread.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Jira configuration.</returns>
    internal static async ValueTask<JiraConfiguration> GetConfigurationAsync(
        IExecutionThread thread, CancellationToken cancellationToken)
    {
        // The host scopes plugin config storage, so "url" is stored as "QueryCat.Plugins.Jira:url".
        var url = await GetStringAsync(thread.ConfigStorage, UrlOption, cancellationToken);
        var username = await GetStringAsync(thread.ConfigStorage, UsernameOption, cancellationToken);

        var authToken = await thread.AuthTokensManager.GetTokenOrDefaultAsync(
            AuthProviderId, cancellationToken: cancellationToken);
        if (authToken == null)
        {
            throw new QueryCatException(
                """
                "Jira authorization is not set. Use 'qcat plugin auth login jira' for OAuth2
                or 'qcat plugin auth login jira --api-key' for API key.");
                """);
        }

        if (IsOAuthToken(authToken))
        {
            return new JiraConfiguration
            {
                BasePath = url,
                OAuthAccessToken = authToken.AccessToken,
                CloudId = await GetCloudIdAsync(authToken.AccessToken, url, cancellationToken),
            };
        }

        // API key: Jira Cloud API token with username or personal access token without it.
        return new JiraConfiguration
        {
            BasePath = RequireUrl(url),
            Username = username,
            AccessToken = authToken.AccessToken,
        };
    }

    /// <summary>
    /// The host does not expose the token type. OAuth2 access tokens from Atlassian always have expiration time
    /// and scopes, API keys have neither.
    /// </summary>
    private static bool IsOAuthToken(AuthToken token) => token.ExpiresAt.HasValue || token.Scopes.Count > 0;

    private static string RequireUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new QueryCatException(
                "Jira URL is not set. Use 'qcat plugin options set jira url https://your-domain.atlassian.net'.");
        }
        return url;
    }

    private static async ValueTask<string> GetStringAsync(IConfigStorage storage, string key,
        CancellationToken cancellationToken)
    {
        var value = await storage.GetOrDefaultAsync(key, cancellationToken: cancellationToken);
        return value.IsNull ? string.Empty : value.AsString.Trim();
    }

    private static async ValueTask<string> GetCloudIdAsync(
        string accessToken, string basePath, CancellationToken cancellationToken)
    {
        var normalizedBasePath = NormalizeUrl(basePath);
        var cacheKey = accessToken + "|" + normalizedBasePath;
        if (_cloudIdCache.TryGetValue(cacheKey, out var cachedCloudId))
        {
            return cachedCloudId;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, AccessibleResourcesUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new QueryCatException(
                $"Cannot get accessible Jira Cloud sites: {response.StatusCode} {response.ReasonPhrase}. {content}");
        }

        if (JsonNode.Parse(content) is not JsonArray resources)
        {
            throw new QueryCatException("Atlassian returned an invalid accessible resources response.");
        }

        JsonNode? resource;
        if (!string.IsNullOrEmpty(normalizedBasePath))
        {
            resource = resources.FirstOrDefault(r => string.Equals(
                NormalizeUrl(r?["url"]?.GetValue<string>()), normalizedBasePath, StringComparison.OrdinalIgnoreCase));
            if (resource == null)
            {
                throw new QueryCatException(
                    $"The Jira site '{basePath}' is not authorized. Authorized sites: {FormatSites(resources)}.");
            }
        }
        else if (resources.Count == 1)
        {
            resource = resources[0];
        }
        else
        {
            throw new QueryCatException(resources.Count == 0
                ? "No Jira Cloud sites are authorized. Run 'qcat plugin auth login jira' and select a site."
                : $"Several Jira Cloud sites are authorized ({FormatSites(resources)}). "
                  + "Select one with 'qcat plugin options set jira url <site>'.");
        }

        var cloudId = resource?["id"]?.GetValue<string>();
        if (string.IsNullOrEmpty(cloudId))
        {
            throw new QueryCatException("Atlassian returned a Jira Cloud site without a cloud ID.");
        }
        _cloudIdCache[cacheKey] = cloudId;
        return cloudId;
    }

    private static string FormatSites(JsonArray resources)
        => string.Join(", ", resources.Select(r => r?["url"]?.GetValue<string>()).Where(u => !string.IsNullOrEmpty(u)));

    private static string NormalizeUrl(string? url) => url?.Trim().TrimEnd('/') ?? string.Empty;
}
