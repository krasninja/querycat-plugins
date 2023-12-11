using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Core.IO.Arguments;
using Cake.Frosting;
using QueryCat.Build;

namespace QueryCat.Plugins.Build;

[TaskName("Build-Plugins")]
[TaskDescription("Build all plugins target")]
public class BuildPluginsTask : AsyncFrostingTask<BuildContext>
{
    private const bool PublishAotDefault = false;

    private readonly string[] _platforms =
    {
        DotNetConstants.RidLinuxX64,
        DotNetConstants.RidLinuxArm64,
        DotNetConstants.RidWindowsX64,
    };

    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        var targetPlatform = context.Arguments.GetArgument("Platform");
        var targetProject = context.Arguments.GetArgument("Project");
        var publishAot = bool.Parse(context.Arguments.GetArgument(DotNetConstants.PublishAotArgument)
            ?? PublishAotDefault.ToString());

        foreach (var project in context.Projects)
        {
            if (!string.IsNullOrEmpty(targetProject) && !project.Contains(targetProject, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var platform in _platforms)
            {
                if (!string.IsNullOrEmpty(targetPlatform) && !platform.Contains(targetPlatform, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                context.DotNetPublish(Path.Combine("..", project), new DotNetPublishSettings
                {
                    OutputDirectory = context.OutputDirectory,
                    Runtime = platform,
                    PublishSingleFile = publishAot ? null : true,
                    PublishTrimmed = publishAot,
                    Configuration = DotNetConstants.ConfigurationRelease,
                    NoLogo = true,
                    IncludeAllContentForSelfExtract = publishAot ? null : true,
                    EnableCompressionInSingleFile = publishAot ? null : true,
                    SelfContained = publishAot ? null : true,
                    ArgumentCustomization = pag =>
                    {
                        if (publishAot)
                        {
                            pag.Append(new TextArgument("-p:PublishAot=true"));
                            pag.Append(new TextArgument("-p:OptimizationPreference=Speed"));
                            pag.Append(new TextArgument("-p:StripSymbols=true"));
                        }
                        pag.Append(new TextArgument($"-p:Runtime={platform}"));
                        pag.Append(new TextArgument("-p:UseAssemblyName=true"));
                        pag.Append(new TextArgument("-p:DebuggerSupport=false"));
                        pag.Append(new TextArgument("-p:EnableUnsafeBinaryFormatterSerialization=false"));
                        pag.Append(new TextArgument("-p:InvariantGlobalization=true"));
                        return pag;
                    },
                });
            }
        }

        return Task.CompletedTask;
    }
}
