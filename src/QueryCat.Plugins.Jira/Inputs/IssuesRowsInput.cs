using System.ComponentModel;
using System.Text.Json.Nodes;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
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
internal sealed class IssuesRowsInput : FetchInput<JsonNode>
{
    private string _key = string.Empty;

    public static void InitializeBasicFields(ClassRowsFrameBuilder<JsonNode> builder)
    {
        builder
            .AddDataProperty()
            .AddProperty("id", p => p.GetValueByPath("id").AsString, "The issue identifier.")
            .AddProperty("key", p => p.GetValueByPath("key").AsString, "The issue key.")
            .AddProperty("project_key", p => p.GetValueByPath("fields.project.key").AsString, "The issue project key.")
            .AddProperty("project_name", p => p.GetValueByPath("fields.project.name").AsString, "The issue project name.")
            .AddProperty("status", p => p.GetValueByPath("fields.status.name").AsString, "The issue status.")
            .AddProperty("created", p => p.GetValueByPath("fields.created").AsTimestamp,
                "The issue creation date and time.")
            .AddProperty("creator_account_id", p => p.GetValueByPath("fields.creator.accountId").AsString, "The issue creator account identifier.")
            .AddProperty("creator_display_name", p => p.GetValueByPath("fields.creator.displayName").AsString, "The issue creator name.")
            .AddProperty("summary", p => p.GetValueByPath("fields.summary").AsString, "The issue summary.")
            .AddProperty("priority", p => p.GetValueByPath("fields.priority.name").AsString, "The issue priority.");
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<JsonNode> builder)
    {
        InitializeBasicFields(builder);
        builder
            .AddProperty("description", p => p.GetValueByPath("renderedFields.description").AsString, "The issue description.");

        AddKeyColumn("key",
            isRequired: true,
            set: value => _key = value.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<JsonNode> GetData(Fetcher<JsonNode> fetch)
    {
        var config = General.GetConfiguration(QueryContext.InputConfigStorage);
        var request = new RestRequest("issue/{key}")
            .AddUrlSegment("key", _key)
            .AddQueryParameter("expand", "renderedFields");
        return fetch.FetchOne(ct => Task.FromResult(config.Client.Get(request).ToJson()));
    }
}
