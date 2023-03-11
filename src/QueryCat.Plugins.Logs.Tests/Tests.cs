using Xunit;
using QueryCat.Backend.Execution;
using QueryCat.Tests.QueryRunner;

namespace QueryCat.Plugins.Logs.Tests;

public sealed class Tests : IDisposable
{
    private readonly TestThread _testThread = new();

    [Theory]
    [MemberData(nameof(GetData))]
    public void Select(string fileName)
    {
        // Arrange.
        new ExecutionThreadBootstrapper().Bootstrap(_testThread);
        var data = TestThread.GetQueryData(fileName);
        _testThread.Run(data.Query);

        // Act.
        var result = _testThread.GetQueryResult();

        // Assert.
        Assert.Equal(data.Expected, result);
    }

    public static IEnumerable<object[]> GetData() => TestThread.GetTestFiles();

    public void Dispose()
    {
        _testThread.Dispose();
    }
}
