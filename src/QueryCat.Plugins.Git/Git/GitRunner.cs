using System.Diagnostics;
using System.Text;

namespace QueryCat.Plugins.Git.Git;

/// <summary>
/// The class helps to run Git command locally.
/// </summary>
public sealed class GitRunner
{
    private readonly StringBuilder _args = new();

    public GitRunner(string command)
    {
        _args.Append(command);
        _args.Append(' ');
    }

    /// <summary>
    /// Add argument.
    /// </summary>
    /// <param name="argument">Argument.</param>
    /// <returns>Instance of <see cref="GitRunner" />.</returns>
    public GitRunner AddArgument(string argument)
    {
        AppendQuoted(_args, argument).Append(' ');
        return this;
    }

    /// <summary>
    /// Add argument.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="value">Argument value.</param>
    /// <returns>Instance of <see cref="GitRunner" />.</returns>
    public GitRunner AddArgument(string name, string value)
    {
        _args
            .Append(name)
            .Append('=');
        AppendQuoted(_args, value);
        return this;
    }

    public async Task<string> RunAsync(string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        return await RunGitCommandAsync(_args.ToString(), workingDirectory ?? string.Empty, cancellationToken);
    }

    private static async Task<string> RunGitCommandAsync(string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "git.exe" : "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new GitException("Cannot run Git executable.");
        }
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new GitException("Git failed: " + error);
        }

        return output;
    }

    private static readonly char[] _whiteSpaceChars = [' ', '\r', '\n', '\t'];

    public static StringBuilder AppendQuoted(StringBuilder builder, string s)
    {
        if (NeedsEscaping())
        {
            builder.Append('"').Append(s).Append('"');
        }
        else
        {
            builder.Append(s);
        }

        return builder;

        bool NeedsEscaping()
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                // Quote empty or white space strings.
                return true;
            }

            if (s.IndexOfAny(_whiteSpaceChars) == -1)
            {
                // Doesn't contain any white space.
                return false;
            }

            if (s.Length > 1 && s[0] == '"' && s[^1] == '"')
            {
                // String is already quoted.
                return false;
            }

            return true;
        }
    }
}
