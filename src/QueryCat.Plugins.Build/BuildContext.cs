using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Core;
using Cake.Frosting;

namespace QueryCat.Plugins.Build;

/// <summary>
/// The main build settings (version, etc).
/// </summary>
public class BuildContext : FrostingContext
{
    public ConvertableDirectoryPath OutputDirectory => this.Directory("../../output");

    public IEnumerable<string> Projects { get; } = new[]
    {
        "QueryCat.Plugins.Aws",
        "QueryCat.Plugins.Clipboard",
        "QueryCat.Plugins.FileSystem",
        "QueryCat.Plugins.FluidTemplates",
        "QueryCat.Plugins.GitHub",
        "QueryCat.Plugins.Jira",
        "QueryCat.Plugins.Network",
        "QueryCat.Plugins.Numerology",
        "QueryCat.Plugins.System",
        "QueryCat.Plugins.VStarCam",
    };

    public BuildContext(ICakeContext context) : base(context)
    {
    }
}
