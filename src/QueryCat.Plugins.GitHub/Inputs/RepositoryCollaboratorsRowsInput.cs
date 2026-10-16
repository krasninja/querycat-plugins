using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository collaborators input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/collaborators/collaborators#list-repository-collaborators.
/// </remarks>
internal sealed class RepositoryCollaboratorsRowsInput : BaseRowsInput<User>
{
    [SafeFunction]
    [Description("Return GitHub collaborators of a specific repository.")]
    [FunctionSignature("github_repository_collaborators(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubRepositoryCollaboratorsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new RepositoryCollaboratorsRowsInput(token));
    }

    public RepositoryCollaboratorsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<User> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_repository_collaborator.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("login", p => p.Login, "The login name of the collaborator.")
            .AddProperty("id", p => p.Id, "The unique ID of the collaborator.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the collaborator.")
            .AddProperty("avatar_url", p => p.AvatarUrl, "The avatar URL of the collaborator.")
            .AddProperty("html_url", p => p.HtmlUrl, "The GitHub profile URL of the collaborator.")
            .AddProperty("type", p => p.Type, "The type of the collaborator (User or Bot).")
            .AddProperty("site_admin", p => p.SiteAdmin, "True if the collaborator is a site admin.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("affiliation", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<User> GetDataAsync(Fetcher<User> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));

        var request = new RepositoryCollaboratorListRequest();
        if (TryGetKeyColumnValue("affiliation", VariantValue.Operation.Equals, out var affiliationValue))
        {
            request.Affiliation = Enum.Parse<CollaboratorAffiliation>(affiliationValue.AsString, ignoreCase: true);
        }

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            var collaborators = await Client.Repository.Collaborator.GetAll(owner, repository, request,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            return (IEnumerable<User>)collaborators;
        }, cancellationToken);
    }
}
