using System.ComponentModel;
using Octokit;
using Octokit.Models.Response;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository environments input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/deployments/environments#list-environments.
/// </remarks>
internal sealed class RepositoryEnvironmentsRowsInput : BaseRowsInput<DeploymentEnvironment>
{
    [SafeFunction]
    [Description("Return GitHub environments of a specific repository.")]
    [FunctionSignature("github_repository_environments(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubRepositoryEnvironmentsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new RepositoryEnvironmentsRowsInput(token));
    }

    public RepositoryEnvironmentsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<DeploymentEnvironment> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_repository_environment.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("id", p => p.Id, "The unique ID of the environment.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the environment.")
            .AddProperty("name", p => p.Name, "The name of the environment.")
            .AddProperty("url", p => p.Url, "The API URL of the environment.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the environment.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the environment was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the environment was last updated.")
            .AddKeyColumn("repository_full_name", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<DeploymentEnvironment> GetDataAsync(Fetcher<DeploymentEnvironment> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));

        return fetcher.FetchAllAsync(async ct =>
        {
            var response = await Client.Repository.Environment.GetAll(owner, repository);
            return response.Environments;
        }, cancellationToken);
    }
}
