using QueryCat.Backend.Core;

namespace QueryCat.Plugins.Git.Git;

/// <summary>
/// QueryCat Git exception.
/// </summary>
public sealed class GitException : QueryCatException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public GitException(string message) : base(message)
    {
    }
}
