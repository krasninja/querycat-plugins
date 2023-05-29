using System.ComponentModel;
using System.Text.Json.Nodes;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Plugins.Jira.Utils;
using RestSharp;

namespace QueryCat.Plugins.Jira.Inputs;

/// <summary>
/// Issues search.
/// </summary>
/// <remarks>
/// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-get.
/// </remarks>
[Description("Search issues using JQL.")]
[FunctionSignature("jira_issue_search")]
internal sealed class IssuesSearchRowsInput : FetchInput<JsonNode>
{
    private string _jql = string.Empty;

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<JsonNode> builder)
    {
        IssuesRowsInput.InitializeBasicFields(builder);
        builder
            .AddProperty("jql", _ => _jql, "JQL.");
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        inputInfo
            .AddKeyColumn("jql",
                isRequired: true,
                value => _jql = value.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<JsonNode> GetData(Fetcher<JsonNode> fetch)
    {
        var config = General.GetConfiguration(QueryContext.InputConfigStorage);
        fetch.Limit = 100;
        return fetch.FetchLimitOffset((limit, offset, ct) =>
        {
            var request = new RestRequest("search")
                .AddQueryParameter("jql", _jql)
                .AddQueryParameter("startAt", offset)
                .AddQueryParameter("maxResults", limit);
            var json = config.Client.Get(request).ToJson();
            return Task.FromResult(json["issues"]!.AsArray().Select(n => n!));
        });
    }
}
