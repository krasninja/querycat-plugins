using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
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
    [SafeFunction]
    [Description("Search Github issues and pull requests.")]
    [FunctionSignature("github_search_issues(term?: string = 'is:open archived:false assignee:@me'): object<IRowsInput>")]
    public static VariantValue GitHubSearchIssuesFunction(FunctionCallInfo args)
    {
        var term = args.GetAt(0).AsString;
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new SearchIssuesRowsInput(token, term));
    }

    private readonly string _term;

    public SearchIssuesRowsInput(string token, string term) : base(token)
    {
        _term = term;
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
            .AddProperty("url", p => p.HtmlUrl, "URL to HTML")
            .AddProperty("closed_by_user_id", p => p.ClosedBy?.Id, "The user who closed the issue.")
            .AddProperty("closed_at", p => p.ClosedAt, "The date when issue was closed.")
            .AddProperty("repository_full_name", p => Utils.ExtractRepositoryFullNameFromUrl(p.Url))
            .AddProperty("created_at", p => p.CreatedAt, "Issue creation date.")
            .AddKeyColumn("created_at", VariantValue.Operation.GreaterOrEquals)
            .AddKeyColumn("created_at", VariantValue.Operation.LessOrEquals);
    }

    /// <inheritdoc />
    protected override IEnumerable<Issue> GetData(Fetcher<Issue> fetch)
    {
        var request = !string.IsNullOrEmpty(_term) ? new SearchIssuesRequest(_term) : new SearchIssuesRequest();
        fetch.PageStart = 1;
        DateRange? createdAtRange = null;
        if (this.TryGetKeyColumnValue("created_at", VariantValue.Operation.GreaterOrEquals, out var createdAtStart)
            && this.TryGetKeyColumnValue("created_at", VariantValue.Operation.LessOrEquals, out var createdAtEnd))
        {
            createdAtRange = new DateRange(
                new DateTimeOffset(createdAtStart.AsTimestamp),
                new DateTimeOffset(createdAtEnd.AsTimestamp));
        }

        return fetch.FetchPaged(async (page, limit, ct) =>
            {
                request.Page = page;
                request.PerPage = limit;
                if (createdAtRange != null)
                {
                    request.Created = createdAtRange;
                }
                var data = (await Client.Search.SearchIssues(request)).Items;
                return data;
            });
    }
}
