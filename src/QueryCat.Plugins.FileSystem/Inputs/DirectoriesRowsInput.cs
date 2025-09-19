using System.ComponentModel;
using System.Runtime.CompilerServices;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.FileSystem.Inputs;

internal sealed class DirectoriesRowsInput : AsyncEnumerableRowsInput<DirectoriesRowsInput.DirectoryDto>
{
    [SafeFunction]
    [Description("Return information on directories.")]
    [FunctionSignature("fs_dirs(): object<IRowsInput>")]
    public static VariantValue DirectoriesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new DirectoriesRowsInput());
    }

    public class DirectoryDto
    {
        public string Path { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public FileAttributes Attributes { get; set; }

        public UnixFileMode UnixFileMode { get; set; }
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<DirectoryDto> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty("path", p => p.Path + "/", "Full path of the directory.")
            .AddProperty(p => p.Name, "Name of the directory.")
            .AddProperty(p => p.CreationTime, "Date and time at which the directory has been created (in UTC).")
            .AddProperty(p => p.LastWriteTime, "Date and time at which the directory has been last modified (in UTC).")
            .AddProperty(p => p.LastAccessTime, "Date and time at which the directory has been last accessed (in UTC).")
            .AddProperty(p => p.Attributes, "directory attributes.")
            .AddProperty(p => p.UnixFileMode, "UNIX directory permissions.")
            .AddKeyColumn("path", VariantValue.Operation.Like, VariantValue.Operation.Equals);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<DirectoryDto> GetDataAsync(
        Fetcher<DirectoryDto> fetch,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var path = OperatingSystem.IsWindows() ? @"C:\" : "/";
        var recursive = false;
        if (TryGetKeyColumnValue("path", VariantValue.Operation.Equals, out var likePathValue))
        {
            path = likePathValue.AsString;
            var firstPatternChar = path.IndexOfAny(['%', '?']);
            if (firstPatternChar > -1)
            {
                path = path.Substring(0, firstPatternChar);
                recursive = true;
            }
        }
        else if (TryGetKeyColumnValue("path", VariantValue.Operation.Like, out var pathValue))
        {
            path = pathValue.AsString;
        }

        var directories = Directory.EnumerateDirectories(path, string.Empty,
            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var dir in directories)
        {
            var di = new DirectoryInfo(dir);
            yield return new DirectoryDto
            {
                Path = dir,
                Name = Path.GetFileName(dir),
                CreationTime = di.CreationTimeUtc,
                LastWriteTime = di.LastWriteTimeUtc,
                LastAccessTime = di.LastAccessTimeUtc,
                Attributes = di.Attributes,
                UnixFileMode = di.UnixFileMode,
            };
        }
    }
}
