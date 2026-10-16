using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository deployments input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/deployments/deployments#list-deployments.
/// </remarks>
internal sealed class RepositoryDeploymentsRowsInput : BaseRowsInput<Deployment>
{
    [SafeFunction]
    [Description("Return GitHub deployments of a specific repository.")]
    [FunctionSignature("github_repository_deployments(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubRepositoryDeploymentsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new RepositoryDeploymentsRowsInput(token));
    }

    public RepositoryDeploymentsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Deployment> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_repository_deployment.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("id", p => p.Id, "The unique ID of the deployment.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the deployment.")
            .AddProperty("sha", p => p.Sha, "The SHA of the deployment.")
            .AddProperty("ref", p => p.Ref, "The ref of the deployment.")
            .AddProperty("task", p => p.Task, "The deployment task.")
            .AddProperty("environment", p => p.Environment, "The environment of the deployment.")
            .AddProperty("original_environment", p => p.OriginalEnvironment, "The original environment of the deployment.")
            .AddProperty("description", p => p.Description, "The description of the deployment.")
            .AddProperty("creator_login", p => p.Creator.Login, "The login of the user who created the deployment.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the deployment was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the deployment was last updated.")
            .AddProperty("statuses_url", p => p.StatusesUrl, "The URL for the deployment statuses.")
            .AddProperty("repository_url", p => p.RepositoryUrl, "The URL of the repository.")
            .AddProperty("payload", p => p.Payload, "The payload of the deployment.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("environment", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Deployment> GetDataAsync(Fetcher<Deployment> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            return await Client.Repository.Deployment.GetAll(owner, repository,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        }, cancellationToken);
    }
}
