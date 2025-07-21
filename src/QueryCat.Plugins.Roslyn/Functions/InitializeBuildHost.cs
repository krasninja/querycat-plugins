using System.ComponentModel;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Roslyn.Functions;

internal static class InitializeBuildHost
{
    private const string MsBuildPackageVersion = "4.14.0";
    private const string MsBuildPackageName = "Microsoft.CodeAnalysis.Workspaces.MSBuild";
    private const string MsBuildNetCoreContentDirectory = "BuildHost-netcore";
    private const string MsBuildNetFrameworkContentDirectory = "BuildHost-net472";
    private const string VersionFileName = ".version";
    private const string MsBuildPackageUri = @"https://www.nuget.org/api/v2/package/" + MsBuildPackageName + "/" + MsBuildPackageVersion;

    [Description("Initialize build host for Roslyn.")]
    [FunctionSignature("roslyn_init_build_host(): void")]
    public static async ValueTask<VariantValue> InitializeBuildHostFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        // Because of how the build host is loaded we cannot get it directly from the NuGet package.
        // It must be on the disk, and we have to download package and extract it somewhere.
        // See BuildHostProcessManager.GetNetCoreBuildHostPath method:
        // - https://github.com/dotnet/roslyn/blob/main/src/Workspaces/MSBuild/Core/MSBuild/BuildHostProcessManager.cs#L190

        var logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(InitializeBuildHost));
        Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

        var targetDirectory = Path.Combine(
            Application.GetApplicationDirectory(),
            "Roslyn",
            MsBuildPackageName
        );
        var dllDirectory = Path.Combine(targetDirectory, "lib/net9.0/");

        // Check if we got another version, delete and download newer one.
        if (Directory.Exists(targetDirectory) &&
            await GetVersionAsync(targetDirectory, cancellationToken) != MsBuildPackageVersion)
        {
            logger.LogDebug("Removing '{Directory}'.", targetDirectory);
            Directory.Delete(targetDirectory, recursive: true);
        }

        // Fresh installation.
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);

            // Download MsBuild package and extract it.
            using var httpClient = new HttpClient();
            await using var stream = await httpClient.GetStreamAsync(MsBuildPackageUri, cancellationToken);
            ZipFile.ExtractToDirectory(stream, targetDirectory, overwriteFiles: true);
            logger.LogDebug("Extracted to directory '{Directory}'.", targetDirectory);

            // By default, content directory has another location in the package. Copy it into "lib/net" directory
            // So BuildHostProcessManager can find build host.
            var sourceContentDirectory = Path.Combine(targetDirectory, "contentFiles/any/any/", MsBuildNetCoreContentDirectory);
            var targetContentDirectory = Path.Combine(dllDirectory, MsBuildNetCoreContentDirectory);
            logger.LogDebug("Copied '{Directory1}' to '{Directory2}'.", sourceContentDirectory, targetContentDirectory);
            CopyDirectory(sourceContentDirectory, targetContentDirectory, recursive: true);

            sourceContentDirectory = Path.Combine(targetDirectory, "contentFiles/any/any/", MsBuildNetFrameworkContentDirectory);
            targetContentDirectory = Path.Combine(dllDirectory, MsBuildNetFrameworkContentDirectory);
            logger.LogDebug("Copied '{Directory1}' to '{Directory2}'.", sourceContentDirectory, targetContentDirectory);
            CopyDirectory(sourceContentDirectory, targetContentDirectory, recursive: true);

            await WriteVersionAsync(targetDirectory, cancellationToken);
        }
        else
        {
            logger.LogDebug("The target directory '{Directory}' already exists.", targetDirectory);
        }

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        Assembly? CurrentDomainOnAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            logger.LogDebug("Resolving '{Assembly}'.", args.Name);
            var assemblyName = new AssemblyName(args.Name);
            if (string.IsNullOrEmpty(assemblyName.Name))
            {
                return null;
            }

            if (assemblyName.Name.Equals("Microsoft.CodeAnalysis.Workspaces.MSBuild", StringComparison.InvariantCulture))
            {
                return Assembly.LoadFile(
                    Path.Combine(dllDirectory, "Microsoft.CodeAnalysis.Workspaces.MSBuild.dll"));
            }

            return null;
        }

        return VariantValue.Null;
    }

    private static async Task<string> GetVersionAsync(string targetDirectory, CancellationToken cancellationToken)
    {
        try
        {
            var version = await File.ReadAllTextAsync(Path.Combine(targetDirectory, VersionFileName), cancellationToken);
            return version.Trim();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static async Task WriteVersionAsync(string targetDirectory, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(
            Path.Combine(targetDirectory, VersionFileName), MsBuildPackageVersion, cancellationToken);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Source: https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories#example.

        // Get information about the source directory.
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }

        // Cache directories before we start copying.
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory.
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory.
        foreach (FileInfo file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method.
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
