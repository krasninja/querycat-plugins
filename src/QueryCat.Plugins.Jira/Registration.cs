using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Plugins;
using QueryCat.Backend.Core.Plugins.Auth;
using QueryCat.Plugins.Jira.Inputs;

namespace QueryCat.Plugins.Jira;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
public static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(IssueCommentsRowsInput.JiraIssueCommentsFunction);
        functionsManager.RegisterFunction(IssuesRowsInput.JiraIssueFunction);
        functionsManager.RegisterFunction(IssuesSearchRowsInput.JiraIssueSearchFunction);
    }

    /// <summary>
    /// Register authentication providers. QueryCat host performs login and token refresh.
    /// </summary>
    /// <param name="registry">Authentication providers registry.</param>
    public static void RegisterAuthProviders(IAuthProvidersRegistry registry)
    {
        registry.Register(new AuthProviderInfo
        {
            Id = General.AuthProviderId,
            DisplayName = "Jira",
            ClientId = "UnvaUp6dSaTTRfgvss4OxluHd9hq0nK0",
            AuthorizationUrl = "https://auth.atlassian.com/authorize",
            AllowApiKey = true,
            TokenUrl = "https://querycat-auth-broker.anti-soft.ru/oauth/jira/token",
            Scopes = ["read:jira-work", "read:jira-user", "offline_access"],
            ExtraParams = new Dictionary<string, string>
            {
                ["audience"] = "api.atlassian.com",
                ["prompt"] = "consent",
            },
            RedirectPort = 53123,
        });
    }

    /// <summary>
    /// Register plugin configuration options. The values are stored by QueryCat host
    /// with the plugin name prefix ("QueryCat.Plugins.Jira:url").
    /// </summary>
    /// <param name="registry">Configuration options registry.</param>
    public static void RegisterConfigOptions(IPluginConfigOptionsRegistry registry)
    {
        foreach (var option in General.CreateConfigOptions())
        {
            registry.Register(option);
        }
    }
}
