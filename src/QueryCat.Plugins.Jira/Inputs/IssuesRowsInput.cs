using System.ComponentModel;
using System.Text.Json.Nodes;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Plugins.Jira.Utils;
using RestSharp;

namespace QueryCat.Plugins.Jira.Inputs;

/// <summary>
/// Issues.
/// </summary>
/// <remarks>
/// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issues/#api-rest-api-3-issue-issueidorkey-get.
/// https://github.com/turbot/steampipe-plugin-jira/blob/main/jira/table_jira_issue.go.
/// </remarks>
[Description("Issues are the building blocks of any Jira project.")]
[FunctionSignature("jira_issue")]
internal sealed class IssuesRowsInput : ClassEnumerableInput<JsonNode>
{
    private string _key = string.Empty;

    public static void InitializeBasicFields(ClassRowsFrameBuilder<JsonNode> builder)
    {
#pragma warning disable CS8602
        builder
            .AddProperty("id", p => p["id"]!.GetValue<string>(), "The issue identifier.")
            .AddProperty("key", p => p["key"]!.GetValue<string>(), "The issue key.")
            .AddProperty("project_key", p => p["fields"]["project"]["key"].GetValue<string>(), "The issue project key.")
            .AddProperty("project_name", p => p["fields"]["project"]["name"].GetValue<string>(), "The issue project name.")
            .AddProperty("status", p => p["fields"]["status"]["name"].GetValue<string>(), "The issue status.")
            .AddProperty("created", p => DateTime.Parse(p["fields"]["created"].GetValue<string>()),
                "The issue creation date and time.")
            .AddProperty("creator_account_id", p => p["fields"]["creator"]["accountId"].GetValue<string>(), "The issue creator account identifier.")
            .AddProperty("creator_display_name", p => p["fields"]["creator"]["displayName"].GetValue<string>(), "The issue creator name.")
            .AddProperty("summary", p => p["fields"]["summary"].GetValue<string>(), "The issue summary.")
            .AddProperty("priority", p => p["fields"]["priority"]["name"].GetValue<string>(), "The issue priority.");
#pragma warning restore CS8602
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<JsonNode> builder)
    {
        InitializeBasicFields(builder);
#pragma warning disable CS8602
        builder
            .AddProperty("description", p => p["renderedFields"]["description"].GetValue<string>(), "The issue description.");
#pragma warning restore CS8602
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        inputInfo
            .AddKeyColumn("key",
                isRequired: true,
                value => _key = value.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<JsonNode> GetData(ClassEnumerableInputFetch<JsonNode> fetch)
    {
        var config = General.GetConfiguration(QueryContext.InputConfigStorage);
        var request = new RestRequest("issue/{key}")
            .AddUrlSegment("key", _key)
            .AddQueryParameter("expand", "renderedFields");
        return fetch.FetchOne(ct => Task.FromResult(config.Client.Get(request).ToJson()));
    }
}
