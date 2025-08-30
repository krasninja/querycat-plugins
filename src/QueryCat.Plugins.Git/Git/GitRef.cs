namespace QueryCat.Plugins.Git.Git;

public class GitRef
{
    private const string Separator = "%1E";

    // https://git-scm.com/docs/git-for-each-ref#_field_names.
    public const string Pattern =
        "%(refname)%1E" + // 0
        "%(refname:short)%1E" + // 1
        "%(upstream)%1E" + // 2
        "%(HEAD)%1E" + // 3
        "%(objectname)%1E" + // 4
        "%(contents:subject)%1E" + // 5
        "%(authorname)%1E" + // 6
        "%(authoremail)%1E" + // 7
        "%(authordate:raw)%1E" + // 8
        "%(committername)%1E" + // 9
        "%(committeremail)%1E" + // 10
        "%(committerdate:raw)%1E" + // 11
        "";

    public string FullName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Upstream { get; set; } = string.Empty;

    public string CommitSha1 { get; set; } = string.Empty;

    public string CommitMessage { get; set; } = string.Empty;

    public string AuthorName { get; set; } = string.Empty;

    public string AuthorEmail { get; set; } = string.Empty;

    public DateTimeOffset? AuthorDate { get; set; }

    public string Committer { get; set; } = string.Empty;

    public string CommitterEmail { get; set; } = string.Empty;

    public DateTimeOffset? CommitterDate { get; set; }

    public bool IsRemote => FullName.StartsWith("refs/remotes/");

    public bool IsMerged { get; set; }

    public bool IsCurrent { get; set; }

    public bool IsTag => FullName.StartsWith("refs/tags/");

    public static IEnumerable<GitRef> Parse(string output)
    {
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var items = line.Split('\u001E');
            yield return new GitRef
            {
                FullName = items[0],
                Name = items[1],
                Upstream = items[2],
                IsCurrent = items[3] == "*",
                CommitSha1 = items[4],
                CommitMessage = items[5],
                AuthorName = items[6],
                AuthorEmail = GitUtils.StripEmail(items[7]),
                AuthorDate = GitUtils.ParseDate(items[8]),
                Committer = items[9],
                CommitterEmail = GitUtils.StripEmail(items[10]),
                CommitterDate = GitUtils.ParseDate(items[11]),
            };
        }
    }
}
