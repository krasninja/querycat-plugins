using QueryCat.Backend.Abstractions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Jira;

/// <summary>
/// AWS functions.
/// </summary>
internal static class General
{
    public const string JiraUsername = "jira-username";
    public const string JiraPassword = "jira-password";
    public const string JiraUrl = "jira-url";
    public const string JiraToken = "jira-token";
    public const string JiraConfig = "jira-config";

    internal static JiraConfiguration GetConfiguration(IInputConfigStorage configStorage)
    {
        if (configStorage.Has(JiraConfig))
        {
            return configStorage.Get(JiraConfig).As<JiraConfiguration>();
        }

        var config = new JiraConfiguration();
        config.BasePath = configStorage.GetOrDefault(JiraUrl, new VariantValue("https://jira.atlassian.com")).AsString;
        if (configStorage.Has(JiraUsername) && configStorage.Has(JiraPassword))
        {
            config.Username = configStorage.Get(JiraUsername).AsString;
            config.Password = configStorage.Get(JiraPassword).AsString;
        }
        if (configStorage.Has(JiraToken))
        {
            config.AccessToken = configStorage.Get(JiraToken).AsString;
        }

        configStorage.Set(JiraConfig, VariantValue.CreateFromObject(config));
        return config;
    }
}
