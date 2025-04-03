using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub pull request files input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/pulls/pulls?list-pull-requests-files.
/// </remarks>
internal sealed class PullRequestFilesRowsInput : BaseRowsInput<PullRequestFile>
{
    [SafeFunction]
    [Description("Return GitHub files for the specific pull request.")]
    [FunctionSignature("github_pull_files(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> PullRequestFilesFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new PullRequestFilesRowsInput(token));
    }

    public PullRequestFilesRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PullRequestFile> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("pull_number", DataType.Integer, _ => GetKeyColumnValue("pull_number"), "Pull request number.")
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository.")
            .AddProperty("file_name", p => p.FileName, "File name.")
            .AddProperty("status", p => p.Status, "Status, one of: added, removed, modified, renamed, copied, changed, unchanged.")
            .AddProperty("additions", p => p.Additions, "Number of additions.")
            .AddProperty("deletions", p => p.Deletions, "Number of deletions.")
            .AddProperty("changes", p => p.Changes, "Number of changes.")
            .AddProperty("patch", p => p.Patch, "File patch.")
            .AddProperty("blob_url", p => p.BlobUrl, "BLOB URL.")
            .AddProperty("contents_url", p => p.ContentsUrl, "Contents URL.")
            .AddProperty("raw_url", p => p.RawUrl, "Raw URL.")
            .AddProperty("sha", p => p.Sha, "File SHA.")
            .AddProperty("previous_file_name", p => p.PreviousFileName, "Previous file name.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("pull_number", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<PullRequestFile> GetDataAsync(Fetcher<PullRequestFile> fetcher,
        CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name"));
        var pullNumber = GetKeyColumnValue("pull_number").ToInt32();
        return fetcher.FetchAllAsync(
            async ct => await Client.PullRequest.Files(owner, repository, pullNumber), cancellationToken);
    }
}
