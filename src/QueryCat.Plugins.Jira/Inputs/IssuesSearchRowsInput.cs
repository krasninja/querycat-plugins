using System.ComponentModel;
using System.Text.Json.Nodes;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Jira.Utils;
using RestSharp;

namespace QueryCat.Plugins.Jira.Inputs;

/// <summary>
/// Issues search.
/// </summary>
/// <remarks>
/// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-get.
/// </remarks>
internal sealed class IssuesSearchRowsInput : FetchRowsInput<JsonNode>
{
    [SafeFunction]
    [Description("Search issues using JQL.")]
    [FunctionSignature("jira_issue_search(): object<IRowsInput>")]
    public static VariantValue JiraIssueSearchFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new IssuesSearchRowsInput());
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<JsonNode> builder)
    {
        IssuesRowsInput.InitializeBasicFields(builder);
        builder
            .AddProperty("jql", _ => GetKeyColumnValue("jql"), "JQL.")
            .AddKeyColumn("jql", isRequired: true);
    }

    /// <inheritdoc />
    protected override IEnumerable<JsonNode> GetData(Fetcher<JsonNode> fetch)
    {
        var config = General.GetConfiguration(QueryContext.InputConfigStorage);
        fetch.Limit = 100;
        return fetch.FetchLimitOffset((limit, offset, ct) =>
        {
            var request = new RestRequest("search")
                .AddQueryParameter("jql", GetKeyColumnValue("jql"))
                .AddQueryParameter("startAt", offset)
                .AddQueryParameter("maxResults", limit);
            var json = config.Client.Get(request).ToJson();
            return Task.FromResult(json["issues"]!.AsArray().Select(n => n!));
        });
    }
}
