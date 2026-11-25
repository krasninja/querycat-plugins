using RestSharp;
using RestSharp.Authenticators;

namespace QueryCat.Plugins.Jira;

public sealed class JiraConfiguration : IDisposable
{
    private RestClient? _client;

    public string BasePath { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string OAuthAccessToken { get; set; } = string.Empty;

    public string CloudId { get; set; } = string.Empty;

    public RestClient Client
    {
        get
        {
            if (_client == null)
            {
                var baseUrl = !string.IsNullOrEmpty(OAuthAccessToken)
                    ? $"https://api.atlassian.com/ex/jira/{CloudId}/rest/api/3/"
                    : $"{BasePath.TrimEnd('/')}/rest/api/3/";
                _client = new RestClient(baseUrl, options =>
                {
                    if (!string.IsNullOrEmpty(OAuthAccessToken))
                    {
                        options.Authenticator = new JwtAuthenticator(OAuthAccessToken);
                    }
                    else if (!string.IsNullOrEmpty(AccessToken))
                    {
                        // With username it is a Jira Cloud API token (basic authentication),
                        // without username it is a personal access token (bearer authentication).
                        options.Authenticator = string.IsNullOrEmpty(Username)
                            ? new JwtAuthenticator(AccessToken)
                            : new HttpBasicAuthenticator(Username, AccessToken);
                    }
                });
            }
            return _client;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client?.Dispose();
    }
}
