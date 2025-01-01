using System.ComponentModel;
using System.Text.Json.Nodes;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using RestSharp;
using QueryCat.Plugins.Jira.Utils;

namespace QueryCat.Plugins.Jira.Inputs;

/// <summary>
/// Issue comments.
/// </summary>
/// <remarks>
/// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-comments/#api-rest-api-3-issue-issueidorkey-comment-get.
/// </remarks>
internal sealed class IssueCommentsRowsInput : AsyncEnumerableRowsInput<JsonNode>
{
    [SafeFunction]
    [Description("Get issue comments.")]
    [FunctionSignature("jira_issue_comments(): object<IRowsInput>")]
    public static VariantValue JiraIssueCommentsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new IssueCommentsRowsInput());
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<JsonNode> builder)
    {
        builder
            .AddDataPropertyAsJson(p => p)
            .AddProperty("id", p => p.GetValueByPath("id").AsString, "The comment identifier.")
            .AddProperty("issue_id", DataType.Integer, _ => GetKeyColumnValue("issue_id"), "The comment issue identifier.")
            .AddProperty("author_account_id", p => p.GetValueByPath("author.accountId").AsString, "Author account id.")
            .AddProperty("author_display_name", p => p.GetValueByPath("author.displayName").AsString, "Author name.")
            .AddProperty("author_email_address", p => p.GetValueByPath("author.emailAddress").AsString, "Author email.")
            .AddProperty("update_author_account_id", p => p.GetValueByPath("updateAuthor.accountId").AsString, "Update author account id.")
            .AddProperty("update_author_display_name", p => p.GetValueByPath("updateAuthor.displayName").AsString, "Update author name.")
            .AddProperty("update_email_address", p => p.GetValueByPath("updateAuthor.emailAddress").AsString, "Update author email.")
            .AddProperty("body", p => p.GetValueByPath("renderedBody").AsString, "Comment body.")
            .AddProperty("created", p => p.GetValueByPath("created").AsTimestamp, "Creation date and time.")
            .AddProperty("updated", p => p.GetValueByPath("updated").AsTimestamp, "Update date and time.")
            .AddKeyColumn("issue_id", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<JsonNode> GetDataAsync(Fetcher<JsonNode> fetcher,
        CancellationToken cancellationToken = default)
    {
        var config = General.GetConfiguration(QueryContext.InputConfigStorage);
        fetcher.Limit = 100;
        return fetcher.FetchLimitOffsetAsync(async (limit, offset, ct) =>
        {
            var request = new RestRequest("issue/{issueIdOrKey}/comment")
                .AddUrlSegment("issueIdOrKey", GetKeyColumnValue("issue_id").AsString)
                .AddQueryParameter("startAt", offset)
                .AddQueryParameter("maxResults", limit)
                .AddQueryParameter("expand", "renderedBody");
            var json = (await config.Client.GetAsync(request, cancellationToken: ct)).ToJson();
            return json["comments"]!.AsArray().Select(n => n!);
        }, cancellationToken);
    }
}
