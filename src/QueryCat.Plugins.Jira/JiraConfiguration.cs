using RestSharp;
using RestSharp.Authenticators;

namespace QueryCat.Plugins.Jira;

public sealed class JiraConfiguration : IDisposable
{
    private RestClient? _client;

    public string BasePath { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public RestClient Client
    {
        get
        {
            if (_client == null)
            {
                _client = new RestClient($"{BasePath}/rest/api/3/", options =>
                {
                    options.Authenticator = new HttpBasicAuthenticator(Username, Password);
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
