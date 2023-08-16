using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Frosting;

namespace QueryCat.Plugins.Build;

[TaskName("Pack-Plugins")]
[TaskDescription("Pack all plugins target")]
public class PackPluginsTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        foreach (var project in context.Projects)
        {
            context.DotNetPack(Path.Combine("..", project), new DotNetPackSettings
            {
                OutputDirectory = context.OutputDirectory,
                Configuration = DotNetConstants.ConfigurationRelease,
                NoLogo = true
            });
        }

        return Task.CompletedTask;
    }
}
