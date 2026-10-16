using System.ComponentModel;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub repository tree input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/git/trees#get-a-tree.
/// </remarks>
internal sealed class TreeRowsInput : BaseRowsInput<TreeItem>
{
    [SafeFunction]
    [Description("Return GitHub tree entries of a specific repository and SHA.")]
    [FunctionSignature("github_tree(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubTreeFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new TreeRowsInput(token));
    }

    public TreeRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<TreeItem> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_tree.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("repository_full_name", DataType.String, _ => GetKeyColumnValue("repository_full_name"), "The full name of the repository (owner/repo).")
            .AddProperty("tree_sha", DataType.String, _ => GetKeyColumnValue("tree_sha"), "The SHA of the tree.")
            .AddProperty("recursive", DataType.Boolean, _ => GetKeyColumnValue("recursive"), "If true, return objects or subtrees referenced by the tree.")
            .AddProperty("path", p => p.Path, "The file path referenced in the tree.")
            .AddProperty("mode", p => p.Mode, "The file mode (100644 for blob, 100755 for executable, 040000 for tree, 160000 for commit, 120000 for symlink).")
            .AddProperty("type", p => p.Type, "The type of object (blob, tree, or commit).")
            .AddProperty("sha", p => p.Sha, "The SHA1 checksum ID of the object in the tree.")
            .AddProperty("size", p => p.Size, "The size of the blob in bytes.")
            .AddProperty("url", p => p.Url, "The URL to the object referenced in the tree.")
            .AddKeyColumn("repository_full_name", isRequired: true)
            .AddKeyColumn("tree_sha", isRequired: true)
            .AddKeyColumn("recursive", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TreeItem> GetDataAsync(Fetcher<TreeItem> fetcher, CancellationToken cancellationToken = default)
    {
        var (owner, repository) = SplitFullRepositoryName(GetKeyColumnValue("repository_full_name").AsString);
        var treeSha = GetKeyColumnValue("tree_sha").AsString;
        var recursive = GetKeyColumnValue("recursive");
        var isRecursive = !recursive.IsNull && recursive.AsBoolean;

        return fetcher.FetchAllAsync(async ct =>
        {
            TreeResponse treeResponse;
            if (isRecursive)
            {
                treeResponse = await Client.Git.Tree.GetRecursive(owner, repository, treeSha);
            }
            else
            {
                treeResponse = await Client.Git.Tree.Get(owner, repository, treeSha);
            }

            return treeResponse.Tree;
        }, cancellationToken);
    }
}
