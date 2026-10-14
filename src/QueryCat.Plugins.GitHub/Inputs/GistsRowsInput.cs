using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub gists input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/gists/gists.
/// </remarks>
internal sealed class GistsRowsInput : BaseRowsInput<Gist>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(GistsRowsInput));

    [SafeFunction]
    [Description("Return GitHub gists.")]
    [FunctionSignature("github_gists(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubGistsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new GistsRowsInput(token));
    }

    public GistsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Gist> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_gist.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "The unique id of the gist.")
            .AddProperty("description", p => p.Description, "The gist description.")
            .AddProperty("public", p => p.Public, "If true, the gist is public, otherwise it is private.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the gist.")
            .AddProperty("comments", p => p.Comments, "The number of comments for the gist.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the gist was created.")
            .AddProperty("git_pull_url", p => p.GitPullUrl, "The https url to pull or clone the gist.")
            .AddProperty("git_push_url", p => p.GitPushUrl, "The https url to push the gist.")
            .AddProperty("node_id", p => p.NodeId, "The Node ID of the gist.")
            .AddProperty("owner_id", p => p.Owner.Id, "The user id (number) of the gist owner.")
            .AddProperty("owner_login", p => p.Owner.Login, "The user login name of the gist owner.")
            .AddProperty("owner_type", p => p.Owner.Type, "The type of the gist owner (User or Organization).")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the gist was last updated.")
            .AddProperty("files", p => p.Files, "Files in the gist.")
            .AddKeyColumn("id", isRequired: false)
            .AddKeyColumn("user_login", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Gist> GetDataAsync(Fetcher<Gist> fetcher,
        CancellationToken cancellationToken = default)
    {
        var id = GetKeyColumnValue("id");
        var userLogin = GetKeyColumnValue("user_login");

        if (!id.IsNull)
        {
            return fetcher.FetchOneAsync(
                async ct =>
                {
                    _logger.LogDebug("Get gist by id = {GistId}.", id);
                    return await Client.Gist.Get(id.AsString);
                }, cancellationToken);
        }
        else if (!userLogin.IsNull)
        {
            fetcher.PageStart = 1;
            return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            {
                _logger.LogDebug("Get gists for user = {UserLogin}.", userLogin);
                return await Client.Gist.GetAllForUser(userLogin.AsString,
                    new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            }, cancellationToken);
        }
        else
        {
            fetcher.PageStart = 1;
            return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            {
                _logger.LogDebug("Get gists for the authenticated user.");
                return await Client.Gist.GetAll(
                    new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            }, cancellationToken);
        }
    }
}
