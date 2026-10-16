using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub user input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/users/users#get-a-user.
/// </remarks>
internal sealed class UsersRowsInput : BaseRowsInput<User>
{
    [SafeFunction]
    [Description("Return GitHub user information by login.")]
    [FunctionSignature("github_user(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubUserFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new UsersRowsInput(token));
    }

    public UsersRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<User> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_user.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("login", p => p.Login, "The login name of the user.")
            .AddProperty("id", p => p.Id, "The unique ID of the user.")
            .AddProperty("node_id", p => p.NodeId, "The node ID of the user.")
            .AddProperty("avatar_url", p => p.AvatarUrl, "The avatar URL of the user.")
            .AddProperty("html_url", p => p.HtmlUrl, "The GitHub profile URL of the user.")
            .AddProperty("type", p => p.Type, "The type of account (User or Organization).")
            .AddProperty("site_admin", p => p.SiteAdmin, "True if the user is a site admin.")
            .AddProperty("name", p => p.Name, "The name of the user.")
            .AddProperty("company", p => p.Company, "The company of the user.")
            .AddProperty("blog", p => p.Blog, "The blog URL of the user.")
            .AddProperty("location", p => p.Location, "The location of the user.")
            .AddProperty("email", p => p.Email, "The email of the user.")
            .AddProperty("hireable", p => p.Hireable, "True if the user is hireable.")
            .AddProperty("bio", p => p.Bio, "The biography of the user.")
            .AddProperty("public_repos", p => p.PublicRepos, "The number of public repositories.")
            .AddProperty("public_gists", p => p.PublicGists, "The number of public gists.")
            .AddProperty("followers", p => p.Followers, "The number of followers.")
            .AddProperty("following", p => p.Following, "The number of users following.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the user was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the user was last updated.")
            .AddKeyColumn("login", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<User> GetDataAsync(Fetcher<User> fetcher, CancellationToken cancellationToken = default)
    {
        var login = GetKeyColumnValue("login").AsString;

        return fetcher.FetchOneAsync(async ct =>
        {
            return await Client.User.Get(login);
        }, cancellationToken);
    }
}
