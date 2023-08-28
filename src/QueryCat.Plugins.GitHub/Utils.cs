using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;

namespace QueryCat.Plugins.Github;

/// <summary>
/// GitHub utils.
/// </summary>
internal static class Utils
{
    private static readonly ILogger Logger = Application.LoggerFactory.CreateLogger(typeof(Utils));

    public static string ExtractRepositoryFullNameFromUrl(string url)
    {
        // Example: https://api.github.com/repos/saritasa-nest/ats-dart/issues/641.
        var arr = url.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (arr.Length < 5)
        {
            Logger.LogWarning("Incorrect full URL {Url}!", url);
            return string.Empty;
        }
        return $"{arr[3]}/{arr[4]}";
    }
}
