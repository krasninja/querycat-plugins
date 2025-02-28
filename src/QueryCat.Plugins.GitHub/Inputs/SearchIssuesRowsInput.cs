using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub search issues input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/search#search-issues-and-pull-requests.
/// https://docs.github.com/en/search-github/searching-on-github/searching-issues-and-pull-requests.
/// </remarks>
internal sealed class SearchIssuesRowsInput : BaseRowsInput<Issue>
{
    private const string TermKey = "term";
    private const string DefaultQuery = "is:open archived:false assignee:@me";

    [SafeFunction]
    [Description("Search GitHub issues and pull requests.")]
    [FunctionSignature("github_search_issues(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubSearchIssuesFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new SearchIssuesRowsInput(token, string.Empty));
    }

    private readonly string _term;

    public SearchIssuesRowsInput(string token, string term) : base(token)
    {
        _term = term;
    }

    private VariantValue GetTerm()
    {
        if (TryGetKeyColumnValue(TermKey, VariantValue.Operation.Equals, out var term))
        {
            return term;
        }
        return new VariantValue(_term);
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Issue> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_search_issue.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "Issue id.")
            .AddProperty("title", p => p.Title, "Issue title.")
            .AddProperty("body", p => p.Body, "Issue body.")
            .AddProperty("state", p => p.State.StringValue, "Issue state.")
            .AddProperty("author", p => p.User.Login, "Issue author.")
            .AddProperty("comments", p => p.Comments, "Number of comments.")
            .AddProperty("number", p => p.Number, "Issue number.")
            .AddProperty("url", p => p.HtmlUrl, "URL to HTML.")
            .AddProperty("closed_by_user_id", p => p.ClosedBy?.Id, "The user who closed the issue.")
            .AddProperty("closed_at", p => p.ClosedAt, "The date when issue was closed.")
            .AddProperty("repository_full_name", p => Utils.ExtractRepositoryFullNameFromUrl(p.Url))
            .AddProperty("created_at", p => p.CreatedAt, "Issue creation date.")
            .AddProperty(TermKey, DataType.String, _ => GetKeyColumnValue("term"))
            .AddKeyColumn("created_at", VariantValue.Operation.GreaterOrEquals)
            .AddKeyColumn("created_at", VariantValue.Operation.LessOrEquals)
            .AddKeyColumn("closed_at", VariantValue.Operation.GreaterOrEquals)
            .AddKeyColumn("closed_at", VariantValue.Operation.LessOrEquals)
            .AddKeyColumn(TermKey, VariantValue.Operation.Equals);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Issue> GetDataAsync(Fetcher<Issue> fetcher,
        CancellationToken cancellationToken = default)
    {
        var request = !string.IsNullOrEmpty(_term) ? new SearchIssuesRequest(_term) : new SearchIssuesRequest(DefaultQuery);
        if (this.TryGetKeyColumnValue(TermKey, VariantValue.Operation.Equals, out var term))
        {
            request = new SearchIssuesRequest(term);
        }
        fetcher.PageStart = 1;

        DateRange? createdAtRange = null;
        if (this.TryGetKeyColumnValue("created_at", VariantValue.Operation.GreaterOrEquals, out var createdAtStart)
            && this.TryGetKeyColumnValue("created_at", VariantValue.Operation.LessOrEquals, out var createdAtEnd))
        {
            createdAtRange = new DateRange(
                new DateTimeOffset(createdAtStart.ToDateTime()),
                new DateTimeOffset(createdAtEnd.ToDateTime()));
        }

        DateRange? closedAtRange = null;
        if (this.TryGetKeyColumnValue("closed_at", VariantValue.Operation.GreaterOrEquals, out var closedAtStart)
            && this.TryGetKeyColumnValue("closed_at", VariantValue.Operation.LessOrEquals, out var closedAtEnd))
        {
            closedAtRange = new DateRange(
                new DateTimeOffset(closedAtStart.ToDateTime()),
                new DateTimeOffset(closedAtEnd.ToDateTime()));
        }

        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            {
                request.Page = page;
                request.PerPage = limit;
                request.Created ??= createdAtRange;
                request.Closed ??= closedAtRange;
                var data = (await Client.Search.SearchIssues(request)).Items;
                return data;
            }, cancellationToken);
    }
}
