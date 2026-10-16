using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository contents input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/repos/contents#get-repository-content.
/// </remarks>
internal sealed class RepositoryContentsRowsInput : BaseRowsInput<RepositoryContent>
{
    [SafeFunction]
    [Description("Return GitHub contents of a specific repository path.")]
    [FunctionSignature("github_repository_contents(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubRepositoryContentsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new RepositoryContentsRowsInput(token));
    }

    public RepositoryContentsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<RepositoryContent> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_repository_content.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("type", p => p.Type.Value, "The type of content (file, dir, symlink, submodule).")
            .AddProperty("name", p => p.Name, "The name of the content.")
            .AddProperty("path", p => p.Path, "The path of the content.")
            .AddProperty("sha", p => p.Sha, "The SHA of the content.")
            .AddProperty("size", p => p.Size, "The size of the content in bytes.")
            .AddProperty("encoding", p => p.Encoding, "The encoding of the content (typically base64 for files).")
            .AddProperty("content", p => p.Content, "The decoded content of the file.")
            .AddProperty("url", p => p.Url, "The API URL of the content.")
            .AddProperty("git_url", p => p.GitUrl, "The Git URL of the content.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the content.")
            .AddProperty("download_url", p => p.DownloadUrl, "The download URL of the content.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("path", isRequired: false)
            .AddKeyColumn("ref", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<RepositoryContent> GetDataAsync(Fetcher<RepositoryContent> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var path = GetKeyColumnValue("path");
        var refValue = GetKeyColumnValue("ref");
        var pathStr = path.IsNull ? string.Empty : path.AsString;

        return fetcher.FetchAllAsync(async ct =>
        {
            if (!refValue.IsNull)
            {
                return await Client.Repository.Content.GetAllContentsByRef(owner, repository, pathStr, refValue.AsString);
            }
            else
            {
                return await Client.Repository.Content.GetAllContents(owner, repository, pathStr);
            }
        }, cancellationToken);
    }
}
