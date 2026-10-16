using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository tags input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/git/tags.
/// </remarks>
internal sealed class TagsRowsInput : BaseRowsInput<RepositoryTag>
{
    [SafeFunction]
    [Description("Return GitHub tags of a specific repository.")]
    [FunctionSignature("github_tags(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubTagsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new TagsRowsInput(token));
    }

    public TagsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<RepositoryTag> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_tag.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("name", p => p.Name, "The name of the tag.")
            .AddProperty("commit_sha", p => p.Commit.Sha, "The SHA of the commit the tag points to.")
            .AddProperty("commit_url", p => p.Commit.Url, "The URL of the commit the tag points to.")
            .AddProperty("zipball_url", p => p.ZipballUrl, "The URL of the zipball archive for the tag.")
            .AddProperty("tarball_url", p => p.TarballUrl, "The URL of the tarball archive for the tag.")
            .AddKeyColumn("repository_full_name", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<RepositoryTag> GetDataAsync(Fetcher<RepositoryTag> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            var tags = await Client.Repository.GetAllTags(owner, repository,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
            return tags;
        }, cancellationToken);
    }
}
