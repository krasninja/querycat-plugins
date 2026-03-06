using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using RestSharp;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Jira.Utils;

namespace QueryCat.Plugins.Jira.Inputs;

/// <summary>
/// Issues search.
/// </summary>
/// <remarks>
/// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-get.
/// </remarks>
internal sealed class IssuesSearchRowsInput : AsyncEnumerableRowsInput<JsonNode>
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
    protected override async IAsyncEnumerable<JsonNode> GetDataAsync(Fetcher<JsonNode> fetcher,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var config = await General.GetConfigurationAsync(QueryContext.ConfigStorage, cancellationToken);
        var nextPageToken = string.Empty;
        var list = fetcher.FetchUntilHasMoreAsync(async (ct) =>
        {
            var request = new RestRequest("search/jql", Method.Post)
                .AddParameter("jql", GetKeyColumnValue("jql"))
                .AddParameter("expand", "names")
                .AddParameter("fields", "*all,-comment,-description")
                .AddParameter("nextPageToken", nextPageToken)
                .AddParameter("maxResults", 5000);
            var json = (await config.Client.GetAsync(request, ct)).ToJson();
            var hasMore = !json["isLast"]!.GetValue<bool>();
            var nextPageTokenNode = json["nextPageToken"];
            nextPageToken = nextPageTokenNode != null ? nextPageTokenNode.GetValue<string>() : string.Empty;
            return (json["issues"]!.AsArray().Select(n => n!), hasMore);
        }, cancellationToken);
        await foreach (var item in list)
        {
            yield return item;
        }
    }
}
