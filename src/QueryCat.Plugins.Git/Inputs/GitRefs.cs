using System.ComponentModel;
using System.Runtime.CompilerServices;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Git.Git;

namespace QueryCat.Plugins.Git.Inputs;

internal sealed class GitRefs : AsyncEnumerableRowsInput<GitRef>
{
    [SafeFunction]
    [Description("Get Git refs (branches, tags).")]
    [FunctionSignature("git_refs(work_dir?: string := '.'): object<IRowsIterator>")]
    public static VariantValue GitRefsFunction(IExecutionThread thread)
    {
        var workingDirectory = thread.Stack[0].AsString;
        return VariantValue.CreateFromObject(new GitRefs(workingDirectory));
    }

    private readonly string _workingDirectory;

    public GitRefs(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<GitRef> builder)
    {
        builder
            .AddProperty("full_name", p => p.FullName, "Branch or tag full name.")
            .AddProperty("name", p => p.Name, "Branch or tag short name.")
            .AddProperty("upstream", p => p.Upstream, "The name of a local ref which can be considered \"upstream\".")
            .AddProperty("is_current", p => p.IsCurrent, "Is it the current active ref/branch.")
            .AddProperty("is_remote", p => p.IsRemote, "Is it the remote ref.")
            .AddProperty("is_merged", p => p.IsMerged, "Is it merged to the current ref.")
            .AddProperty("is_tag", p => p.IsTag, "Is it the tag ref.")
            .AddProperty("sha1", p => p.CommitSha1, "Branch or tag SHA-1.")
            .AddProperty("message", p => p.CommitMessage, "Branch or tag message.")
            .AddProperty("author_name", p => p.AuthorName, "Author name.")
            .AddProperty("author_email", p => p.AuthorEmail, "Author email.")
            .AddProperty("author_date", p => p.AuthorDate, "Author date.")
            .AddProperty("committer_name", p => p.AuthorName, "Committer name.")
            .AddProperty("committer_email", p => p.AuthorEmail, "Committer email.")
            .AddProperty("committer_date", p => p.AuthorDate, "Committer date.");
        base.Initialize(builder);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<GitRef> GetDataAsync(Fetcher<GitRef> fetcher,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var runner = new GitRunner("for-each-ref");
        runner.AddArgument("--format", GitRef.Pattern);

        var output = await runner.RunAsync(_workingDirectory, cancellationToken);
        foreach (var branch in GitRef.Parse(output))
        {
            yield return branch;
        }
    }
}
