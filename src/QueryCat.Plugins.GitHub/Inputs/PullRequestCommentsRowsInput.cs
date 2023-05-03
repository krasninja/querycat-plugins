using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub pull request comments input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/comments.
/// </remarks>
[Description("Return Github comments for the specific pull request.")]
[FunctionSignature("github_pull_comments")]
internal class PullRequestCommentsRowsInput : BaseRowsInput<PullRequestReviewComment>
{
    private int _pullNumber;
    private string _owner = string.Empty;
    private string _repository = string.Empty;

    public PullRequestCommentsRowsInput(FunctionCallInfo args)
        : base(args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken))
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequestReviewComment> builder)
    {
        builder
            .AddDataProperty()
            .AddProperty("id", p => p.Id, "Comment id.")
            .AddProperty("pull_id", p => p.PullRequestReviewId, "Pull request id.")
            .AddProperty("pull_number", p => _pullNumber, "Pull request number.")
            .AddProperty("repository_full_name", _ => GetFullRepositoryName(_owner, _repository), "The full name of the repository.")
            .AddProperty("body", p => p.Body, "Comment body.")
            .AddProperty("url", p => p.Url, "Comment URL.")
            .AddProperty("path", p => p.Path, "Relative path of the file the comment is about.")
            .AddProperty("commit_id", p => p.CommitId, "Commit id.")
            .AddProperty("original_commit_id", p => p.OriginalCommitId, "Original commit it.")
            .AddProperty("diff_hunk", p => p.DiffHunk, "Diff hunk the comment is about.")
            .AddProperty("in_reply_to_id", p => p.InReplyToId, "Id of the comment this comment replies to.")
            .AddProperty("position", p => p.Position, "Comment position.")
            .AddProperty("created_by_id", p => p.User.Id, "User id who created the comment.")
            .AddProperty("created_by_login", p => p.User.Login, "User login who created the comment.")
            .AddProperty("created_at", p => p.CreatedAt, "Date of comment creation.")
            .AddProperty("updated_at", p => p.UpdatedAt, "Date of comment update.");
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        inputInfo
            .SetInputArguments(Owner, Repository)
            .AddKeyColumn("repository_full_name",
                isRequired: true,
                action: v => (_owner, _repository) = SplitFullRepositoryName(v.AsString))
            .AddKeyColumn("pull_number",
                isRequired: true,
                action: v => _pullNumber = (int)v.AsInteger);
    }

    /// <inheritdoc />
    protected override IEnumerable<PullRequestReviewComment> GetData(Fetcher<PullRequestReviewComment> fetcher)
    {
        fetcher.PageStart = 1;
        return fetcher.FetchPaged(async (page, limit, ct) =>
        {
            return await Client.PullRequest.ReviewComment.GetAll(_owner, _repository, _pullNumber,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        });
    }
}
