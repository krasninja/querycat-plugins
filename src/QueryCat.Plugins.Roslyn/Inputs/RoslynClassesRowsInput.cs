using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.MSBuild;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;

namespace QueryCat.Plugins.Roslyn.Inputs;

[SafeFunction]
[FunctionSignature("roslyn_projects")]
[Description("Return projects within the solution.")]
internal sealed class RoslynClassesRowsInput : BaseRoslynRowsInput<RoslynClassesRowsInput.RoslynProject>
{
    public sealed class RoslynProject
    {
        [Description("The ID of the project. Multiple Project instances may share the same ID.")]
        public required Guid Id { get; set; }

        [Description("The name of the project. This may be different than the assembly name.")]
        public required string Name { get; set; }

        [Description("The name of the assembly this project represents.")]
        public required string AssemblyName { get; set; }

        [Description("The language associated with the project.")]
        [Column("language")]
        public required string Language { get; set; }

        [Description("The project version. This equates to the version of the project file.")]
        public required string Version { get; set; }

        [Description("The path to the project file or null if there is no project file.")]
        public required string? FilePath { get; set; }

        [Description("The path to the output file, or null if it is not known.")]
        public required string? OutputFilePath { get; set; }
    }

    public RoslynClassesRowsInput(
        [Description("Path to .sln file.")] string solutionPath) : base(solutionPath)
    {
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<RoslynProject> GetDataAsync(
        Fetcher<RoslynProject> fetcher,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(SolutionFilePath, cancellationToken: cancellationToken);
        foreach (var solutionProject in solution.Projects)
        {
            yield return new RoslynProject
            {
                Id = solutionProject.Id.Id,
                Name = solutionProject.Name,
                AssemblyName = solutionProject.AssemblyName,
                Language = solutionProject.Language,
                Version = solutionProject.Version.ToString(),
                FilePath = solutionProject.FilePath,
                OutputFilePath = solutionProject.OutputFilePath,
            };
        }
        workspace.CloseSolution();
    }
}
