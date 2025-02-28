using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira;

/// <summary>
/// AWS functions.
/// </summary>
internal static class General
{
    private const string DefaultJiraUrl = "https://jira.jira.org/";

    public const string JiraUsername = "jira-username";
    public const string JiraPassword = "jira-password";
    public const string JiraUrl = "jira-url";
    public const string JiraToken = "jira-token";
    public const string JiraConfig = "jira-config";

    internal static async ValueTask<JiraConfiguration> GetConfigurationAsync(IInputConfigStorage configStorage, CancellationToken cancellationToken)
    {
        if (await configStorage.HasAsync(JiraConfig, cancellationToken))
        {
            return (await configStorage.GetAsync(JiraConfig, cancellationToken)).AsRequired<JiraConfiguration>();
        }

        var config = new JiraConfiguration();
        config.BasePath = (await configStorage.GetOrDefaultAsync(JiraUrl, new VariantValue(DefaultJiraUrl),
            cancellationToken)).AsString;
        if (await configStorage.HasAsync(JiraUsername, cancellationToken)
            && await configStorage.HasAsync(JiraPassword, cancellationToken))
        {
            config.Username = (await configStorage.GetAsync(JiraUsername, cancellationToken)).AsString;
            config.Password = (await configStorage.GetAsync(JiraPassword, cancellationToken)).AsString;
        }
        if (await configStorage.HasAsync(JiraToken, cancellationToken))
        {
            config.AccessToken = (await configStorage.GetAsync(JiraToken, cancellationToken)).AsString;
        }

        await configStorage.SetAsync(JiraConfig, VariantValue.CreateFromObject(config), cancellationToken);
        return config;
    }
}
