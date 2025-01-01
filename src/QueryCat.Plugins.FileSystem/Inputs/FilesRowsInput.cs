using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileSystemGlobbing;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.FileSystem.Inputs;

internal sealed class FilesRowsInput : AsyncEnumerableRowsInput<FilesRowsInput.FileDto>
{
    [SafeFunction]
    [Description("Return information on files.")]
    [FunctionSignature("fs_files(): object<IRowsInput>")]
    public static VariantValue FilesFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new FilesRowsInput());
    }

    public class FileDto
    {
        public string Path { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public long Size { get; set; }

        public bool IsReadOnly { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public FileAttributes Attributes { get; set; }

        public UnixFileMode UnixFileMode { get; set; }
    }

    private readonly Matcher _matcher = new();

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<FileDto> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty("path", p => p.Path, "Full path of the file.")
            .AddProperty(p => p.Name, "Name of the file.")
            .AddProperty(p => p.Size, "Size of the file, in bytes.")
            .AddProperty(p => p.IsReadOnly, "Is file read only.")
            .AddProperty(p => p.CreationTime, "Date and time at which the file has been created (in UTC).")
            .AddProperty(p => p.LastWriteTime, "Date and time at which the file has been last modified (in UTC).")
            .AddProperty(p => p.LastAccessTime, "Date and time at which the file has been last accessed (in UTC).")
            .AddProperty(p => p.Attributes, "File attributes.")
            .AddProperty(p => p.UnixFileMode, "UNIX file permissions.")
            .AddKeyColumn("path", VariantValue.Operation.Like, isRequired: true);
    }

    private void AddInclude(string path)
    {
        path = path.Replace('%', '*');
        path = path.Replace("?", string.Empty);
        _matcher.AddInclude(path);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<FileDto> GetDataAsync(
        Fetcher<FileDto> fetch,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        AddInclude(GetKeyColumnValue("path").AsString);
        var files = _matcher.GetResultsInFullPath("/");
        foreach (var file in files)
        {
            var fi = new FileInfo(file);
            yield return new FileDto
            {
                Path = file,
                Name = Path.GetFileName(file),
                Size = fi.Length,
                IsReadOnly = fi.IsReadOnly,
                CreationTime = fi.CreationTimeUtc,
                LastWriteTime = fi.LastWriteTimeUtc,
                LastAccessTime = fi.LastAccessTimeUtc,
                Attributes = fi.Attributes,
                UnixFileMode = fi.UnixFileMode,
            };
        }
    }
}
