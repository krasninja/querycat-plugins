using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub pull requests input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/pulls#get-a-pull-request.
/// </remarks>
internal sealed class PullRequestsRowsInput : BaseRowsInput<PullRequest>
{
    [SafeFunction]
    [Description("Return GitHub pull requests of specific repository.")]
    [FunctionSignature("github_pulls(): object<IRowsInput>")]
    public static VariantValue GitHubPullsFunction(IExecutionThread thread)
    {
        var token = thread.ConfigStorage.GetOrDefault(General.GitHubToken);
        return VariantValue.CreateFromObject(new PullRequestsRowsInput(token));
    }

    public PullRequestsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequest> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "Pull request id.")
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("number", p => p.Number, "The pull request issue number.")
            .AddProperty("title", p => p.Title, "Pull request title.")
            .AddProperty("author_login", p => p.User.Login, "The login name of the user that submitted the PR.")
            .AddProperty("state", p => p.State.Value, "The state or the PR (open, closed).")
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
            .AddProperty("mergeable_state", p => p.MergeableState?.Value, "The mergeability state of the PR.")
            .AddProperty("merged", p => p.Merged, "If true, the PR has been merged.")
            .AddProperty("merged_at", p => p.MergedAt, "The timestamp when the PR was merged.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("number", isRequired: false)
            .AddKeyColumn("state");
    }

    /// <inheritdoc />
    protected override IEnumerable<PullRequest> GetData(Fetcher<PullRequest> fetcher)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));

        var number = GetKeyColumnValue("number");
        if (!number.IsNull)
        {
            return fetcher.FetchOne(async ct => await Client.Repository.PullRequest.Get(owner, repository, number));
        }
        else
        {
            fetcher.PageStart = 1;
            var options = new PullRequestRequest
            {
                State = ItemStateFilter.All,
            };
            if (TryGetKeyColumnValue("state", VariantValue.Operation.Equals, out var state))
            {
                options.State = Enum.Parse<ItemStateFilter>(state);
            }
            return fetcher.FetchAll(
                async ct => await Client.PullRequest.GetAllForRepository(owner, repository, options));
        }
    }
}
