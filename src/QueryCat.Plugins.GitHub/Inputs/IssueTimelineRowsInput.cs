using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub list timeline events for an issue.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/issues/timeline.
/// </remarks>
internal sealed class IssueTimelineRowsInput : BaseRowsInput<TimelineEventInfo>
{
    [SafeFunction]
    [Description("Return GitHub timeline for the specific issue.")]
    [FunctionSignature("github_issue_timeline(): object<IRowsInput>")]
    public static VariantValue IssueTimelineFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new IssueTimelineRowsInput(thread));
    }

    public IssueTimelineRowsInput(IExecutionThread thread)
        : base(thread.ConfigStorage.GetOrDefault(General.GitHubToken))
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<TimelineEventInfo> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "Timeline id.")
            .AddProperty("created_at", p => p.CreatedAt, "Creation date and time.")
            .AddProperty("event", p => p.Event.StringValue, "Event.")
            .AddProperty("label_name", p => p.Label.Name, "Label name.")
            .AddProperty("commit_id", p => p.CommitId, "The related commit identifier.")
            .AddProperty("actor_login", p => p.Actor.Login, "Actor login.")
            .AddProperty("actor_email", p => p.Actor.Email, "Actor email.")
            .AddProperty("assignee_login", p => p.Assignee.Login, "Assignee login.")
            .AddProperty("assignee_email", p => p.Assignee.Email, "Assignee email.")
            .AddProperty("number", DataType.Integer, _ => GetKeyColumnValue("number"), "Issue or pull request number.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IEnumerable<TimelineEventInfo> GetData(Fetcher<TimelineEventInfo> fetcher)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var number = (int)GetKeyColumnValue("number").AsInteger;
        return fetcher.FetchAll(
            async ct => await Client.Issue.Timeline.GetAllForIssue(owner, repository, number));
    }
}
