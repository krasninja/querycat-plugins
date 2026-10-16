using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository workflows input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/actions/workflows#list-repository-workflows.
/// </remarks>
internal sealed class WorkflowsRowsInput : BaseRowsInput<Workflow>
{
    [SafeFunction]
    [Description("Return GitHub Actions workflows of a specific repository.")]
    [FunctionSignature("github_workflows(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubWorkflowsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new WorkflowsRowsInput(token));
    }

    public WorkflowsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Workflow> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_workflow.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("id", p => p.Id, "The unique ID of the workflow.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the workflow.")
            .AddProperty("name", p => p.Name, "The name of the workflow.")
            .AddProperty("path", p => p.Path, "The path to the workflow file.")
            .AddProperty("state", p => p.State, "The state of the workflow (active, deleted, disabled_fork, disabled_inactivity, disabled_manually).")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the workflow was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the workflow was last updated.")
            .AddProperty("url", p => p.Url, "The API URL of the workflow.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the workflow.")
            .AddProperty("badge_url", p => p.BadgeUrl, "The badge URL of the workflow.")
            .AddKeyColumn("repository_full_name", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Workflow> GetDataAsync(Fetcher<Workflow> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name").AsString);

        return fetcher.FetchAllAsync(async ct =>
        {
            var workflowsResponse = await Client.Actions.Workflows.List(owner, repository);
            return workflowsResponse.Workflows;
        }, cancellationToken);
    }
}
