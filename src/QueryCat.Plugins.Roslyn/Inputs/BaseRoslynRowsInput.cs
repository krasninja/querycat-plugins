using QueryCat.Backend.Core.Fetch;

namespace QueryCat.Plugins.Roslyn.Inputs;

internal abstract class BaseRoslynRowsInput<T> : AsyncEnumerableRowsInput<T> where T : class
{
    public string SolutionFilePath { get; }

    public BaseRoslynRowsInput(string solutionFilePath)
    {
        SolutionFilePath = solutionFilePath;
    }
}
