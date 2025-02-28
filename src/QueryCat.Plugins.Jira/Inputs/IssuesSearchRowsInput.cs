using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        var config = await General.GetConfigurationAsync(QueryContext.InputConfigStorage, cancellationToken);
        fetcher.Limit = 100;
        var list = fetcher.FetchLimitOffsetAsync(async (limit, offset, ct) =>
        {
            var request = new RestRequest("search")
                .AddQueryParameter("jql", GetKeyColumnValue("jql"))
                .AddQueryParameter("startAt", offset)
                .AddQueryParameter("maxResults", limit);
            var json = (await config.Client.GetAsync(request, ct)).ToJson();
            return json["issues"]!.AsArray().Select(n => n!);
        }, cancellationToken);
        await foreach (var item in list)
        {
            yield return item;
        }
    }
}
