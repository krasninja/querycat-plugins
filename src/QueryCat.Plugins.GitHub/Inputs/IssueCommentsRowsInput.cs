using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub issue comments input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/commits/comments.
/// </remarks>
internal sealed class IssueCommentsRowsInput : BaseRowsInput<IssueComment>
{
    [SafeFunction]
    [Description("Return GitHub comments for the specific issue.")]
    [FunctionSignature("github_issue_comments(): object<IRowsInput>")]
    public static VariantValue IssueCommentsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new IssueCommentsRowsInput(thread));
    }

    public IssueCommentsRowsInput(IExecutionThread thread)
        : base(thread.ConfigStorage.GetOrDefault(General.GitHubToken))
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<IssueComment> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "Comment id.")
            .AddProperty("issue_number", p => GetKeyColumnValue("issue_number"), "Issue number.")
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("body", p => p.Body, "Comment body.")
            .AddProperty("url", p => p.Url, "Comment URL.")
            .AddProperty("created_by_id", p => p.User.Id, "User id who created the comment.")
            .AddProperty("created_by_login", p => p.User.Login, "User login who created the comment.")
            .AddProperty("created_at", p => p.CreatedAt, "Date of comment creation.")
            .AddProperty("updated_at", p => p.UpdatedAt, "Date of comment update.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("issue_number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IEnumerable<IssueComment> GetData(Fetcher<IssueComment> fetcher)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var issueNumber = GetKeyColumnValue("issue_number").ToInt32();
        fetcher.PageStart = 1;
        return fetcher.FetchPaged(async (page, limit, ct) =>
        {
            return await Client.Issue.Comment.GetAllForIssue(owner, repository, issueNumber,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        });
    }
}
