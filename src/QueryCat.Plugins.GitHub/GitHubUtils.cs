namespace QueryCat.Plugins.Github;

/// <summary>
/// GitHub utils.
/// </summary>
internal static class GitHubUtils
{
    public static string ExtractRepositoryFullNameFromUrl(string url)
    {
        // Example: https://api.github.com/repos/saritasa-nest/ats-dart/issues/641.
        var arr = url.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (arr.Length < 5)
        {
            Serilog.Log.Warning("GitHub: Incorrect full URL {Url}!", url);
            return string.Empty;
        }
        return $"{arr[3]}/{arr[4]}";
    }
}
