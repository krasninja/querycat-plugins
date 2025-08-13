using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Core;
using Cake.Frosting;

namespace QueryCat.Plugins.Build;

/// <summary>
/// The main build settings (version, etc).
/// </summary>
public sealed class BuildContext : FrostingContext
{
    public ConvertableDirectoryPath OutputDirectory => this.Directory("../../output");

    public IEnumerable<string> Projects { get; } =
    [
        "QueryCat.Plugins.Aws",
        "QueryCat.Plugins.Clipboard",
        "QueryCat.Plugins.Database",
        "QueryCat.Plugins.FileSystem",
        "QueryCat.Plugins.FluidTemplates",
        "QueryCat.Plugins.GigaChat",
        "QueryCat.Plugins.GitHub",
        "QueryCat.Plugins.Jira",
        "QueryCat.Plugins.Network",
        "QueryCat.Plugins.Ollama",
        "QueryCat.Plugins.OpenAI",
        "QueryCat.Plugins.Numerology",
        "QueryCat.Plugins.PostgresSniffer",
        "QueryCat.Plugins.Roslyn",
        "QueryCat.Plugins.Subtitles",
        "QueryCat.Plugins.System",
        "QueryCat.Plugins.VStarCam",
        "QueryCat.Plugins.WindowsEventLog",
    ];

    public BuildContext(ICakeContext context) : base(context)
    {
    }
}
