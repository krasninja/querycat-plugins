using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub pull requests input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/pulls#get-a-pull-request.
/// </remarks>
internal class PullRequestsRowsInput : BaseRowsInput<PullRequest>
{
    [Description("Return Github pull requests of specific repository.")]
    [FunctionSignature("github_pulls(): object<IRowsInput>")]
    public static VariantValue GitHubPullsFunction(FunctionCallInfo args)
    {
        var token = args.ExecutionThread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new PullRequestsRowsInput(token));
    }

    private string _owner = string.Empty;
    private string _repository = string.Empty;
    private int _number;

    public PullRequestsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequest> builder)
    {
        builder
            .AddDataProperty()
            .AddProperty("repository_full_name", _ => GetFullRepositoryName(_owner, _repository), "The full name of the repository.")
            .AddProperty("number", p => p.Number, "The pull request issue number.")
            .AddProperty("title", p => p.Title, "Pull request title.")
            .AddProperty("author_login", p => p.User.Login, "The login name of the user that submitted the PR.")
            .AddProperty("state", p => p.State, "The state or the PR (open, closed).")
            .AddProperty("body", p => p.Body, "Pull request title.")
            .AddProperty("additions", p => p.Additions, "The number of additions in this PR.")
            .AddProperty("deletions", p => p.Deletions, "The number of deletions in this PR.")
            .AddProperty("changed_files", p => p.ChangedFiles, "The number of changed files.")
            .AddProperty("closed_at", p => p.ClosedAt, "The timestamp when the PR was closed.")
            .AddProperty("comments", p => p.Comments, "The number of comments on the PR.")
            .AddProperty("commits", p => p.Commits, "The number of commits in this PR.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the PR was created.")
            .AddProperty("draft", p => p.Draft, "If true, the PR is in draft.")
            .AddProperty("base_ref", p => p.Base.Ref, "The base branch of the PR in GitHub.")
            .AddProperty("head_ref", p => p.Head.Ref, "The head branch of the PR in GitHub.")
            .AddProperty("locked", p => p.Locked, "If true, the PR is locked.")
            .AddProperty("maintainer_can_modify", p => p.MaintainerCanModify,
                "If true, people with push access to the upstream repository of a fork owned by a user " +
                "account can commit to the forked branches.")
            .AddProperty("mergeable", p => p.Mergeable, "If true, the PR can be merged.")
            .AddProperty("mergeable_state", p => p.MergeableState, "The mergeability state of the PR.")
            .AddProperty("merged", p => p.Merged, "If true, the PR has been merged.")
            .AddProperty("merged_at", p => p.MergedAt, "The timestamp when the PR was merged.");
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        inputInfo
            .SetInputArguments(Owner, Repository)
            .AddKeyColumn("repository_full_name",
                isRequired: true,
                action: v => (_owner, _repository) = SplitFullRepositoryName(v.AsString))
            .AddKeyColumn("number",
                isRequired: true,
                action: v => _number = (int)v.AsInteger);
    }

    /// <inheritdoc />
    protected override IEnumerable<PullRequest> GetData(Fetcher<PullRequest> fetch)
    {
        return fetch.FetchOne(async ct => await Client.Repository.PullRequest.Get(_owner, _repository, _number));
    }
}
