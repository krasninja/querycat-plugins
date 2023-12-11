using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub issue comments input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/commits/comments.
/// </remarks>
[Description("Return Github comments for the specific issue.")]
[FunctionSignature("github_issue_comments")]
internal sealed class IssueCommentsRowsInput : BaseRowsInput<IssueComment>
{
    private int _issueNumber;
    private string _owner = string.Empty;
    private string _repository = string.Empty;

    public IssueCommentsRowsInput(FunctionCallInfo args)
        : base(args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken))
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<IssueComment> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "Comment id.")
            .AddProperty("issue_number", p => _issueNumber, "Issue number.")
            .AddProperty("repository_full_name", _ => GetFullRepositoryName(_owner, _repository), "The full name of the repository.")
            .AddProperty("body", p => p.Body, "Comment body.")
            .AddProperty("url", p => p.Url, "Comment URL.")
            .AddProperty("created_by_id", p => p.User.Id, "User id who created the comment.")
            .AddProperty("created_by_login", p => p.User.Login, "User login who created the comment.")
            .AddProperty("created_at", p => p.CreatedAt, "Date of comment creation.")
            .AddProperty("updated_at", p => p.UpdatedAt, "Date of comment update.");

        AddKeyColumn("repository_full_name",
            isRequired: true,
            set: v => (_owner, _repository) = SplitFullRepositoryName(v.AsString));
        AddKeyColumn("issue_number",
            isRequired: true,
            set: v => _issueNumber = (int)v.AsInteger);
    }

    /// <inheritdoc />
    protected override IEnumerable<IssueComment> GetData(Fetcher<IssueComment> fetcher)
    {
        fetcher.PageStart = 1;
        return fetcher.FetchPaged(async (page, limit, ct) =>
        {
            return await Client.Issue.Comment.GetAllForIssue(_owner, _repository, _issueNumber,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        });
    }
}
