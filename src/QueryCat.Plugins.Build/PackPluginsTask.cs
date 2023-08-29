using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core;
using Cake.Core.IO.Arguments;
using Cake.Frosting;
using QueryCat.Build;

namespace QueryCat.Plugins.Build;

[TaskName("Pack-Plugins")]
[TaskDescription("Pack all plugins target")]
public class PackPluginsTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        var targetProject = context.Arguments.GetArgument("Project");

        foreach (var project in context.Projects)
        {
            if (!string.IsNullOrEmpty(targetProject) && !project.Contains(targetProject, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            context.DotNetPack(Path.Combine("..", project), new DotNetPackSettings
            {
                OutputDirectory = context.OutputDirectory,
                Configuration = DotNetConstants.ConfigurationRelease,
                NoLogo = true,
                ArgumentCustomization = pag =>
                {
                    pag.Append(new TextArgument("-p:UseAssemblyName=false"));
                    pag.Append(new TextArgument("-p:OutputType=Library"));
                    return pag;
                },
            });
        }

        return Task.CompletedTask;
    }
}
