using Xunit;
using QueryCat.Backend.Execution;

namespace QueryCat.Plugins.Logs.Tests;

public sealed class Tests : IDisposable
{
    private readonly Backend.Tests.TestThread _testThread = new();

    [Theory]
    [MemberData(nameof(GetData))]
    public void Select(string fileName)
    {
        // Arrange.
        new ExecutionThreadBootstrapper().Bootstrap(_testThread);
        var data = Backend.Tests.TestThread.GetQueryData(fileName);
        _testThread.Run(data.Query);

        // Act.
        var result = _testThread.GetQueryResult();

        // Assert.
        Assert.Equal(data.Expected, result);
    }

    public static IEnumerable<object[]> GetData() => Backend.Tests.TestThread.GetTestFiles();

    public void Dispose()
    {
        _testThread.Dispose();
    }
}
