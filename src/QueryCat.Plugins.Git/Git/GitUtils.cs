namespace QueryCat.Plugins.Git.Git;

internal static class GitUtils
{
    public static string StripEmail(string email) => email.Trim('<', '>');

    public static DateTimeOffset? ParseDate(string rawDate)
    {
        if (string.IsNullOrWhiteSpace(rawDate))
        {
            return null;
        }

        var parts = rawDate.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1)
        {
            return null;
        }

        // Unix timestamp.
        var seconds = long.Parse(parts[0]);

        // Convert to UTC DateTime.
        var dto = DateTimeOffset.FromUnixTimeSeconds(seconds);

        // NOTE: the second part (offset, e.g. +0200) is redundant
        // because Unix timestamp is always UTC.
        return dto.UtcDateTime;
    }
}
