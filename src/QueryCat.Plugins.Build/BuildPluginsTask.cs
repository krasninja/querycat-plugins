using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core.IO.Arguments;
using Cake.Frosting;
using QueryCat.Build;

namespace QueryCat.Plugins.Build;

[TaskName("Build-Plugins")]
[TaskDescription("Build all plugins target")]
public class BuildPluginsTask : AsyncFrostingTask<BuildContext>
{
    private readonly string[] _platforms =
    {
        DotNetConstants.RidLinuxX64,
        DotNetConstants.RidLinuxArm64,
        DotNetConstants.RidWindowsX64,
    };

    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        foreach (var project in context.Projects)
        {
            foreach (var platform in _platforms)
            {
                context.DotNetPublish(Path.Combine("..", project), new DotNetPublishSettings
                {
                    OutputDirectory = context.OutputDirectory,
                    Runtime = platform,
                    PublishSingleFile = true,
                    PublishTrimmed = true,
                    Configuration = DotNetConstants.ConfigurationRelease,
                    NoLogo = true,
                    IncludeAllContentForSelfExtract = false,
                    EnableCompressionInSingleFile = true,
                    SelfContained = true,
                    ArgumentCustomization = pag =>
                    {
                        pag.Append(new TextArgument($"-p:Runtime={platform}"));
                        pag.Append(new TextArgument("-p:DebuggerSupport=false"));
                        pag.Append(new TextArgument("-p:EnableUnsafeBinaryFormatterSerialization=false"));
                        return pag;
                    }
                });
            }
        }

        return Task.CompletedTask;
    }
}
