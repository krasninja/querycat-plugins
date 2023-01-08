using Octokit;
using QueryCat.Backend;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.Github;

internal abstract class BaseRowsInput<TClass> : ClassEnumerableInput<TClass> where TClass : class
{
    protected string Owner { get; } = string.Empty;

    protected string Repository { get; } = string.Empty;

    protected GitHubClient Client { get; }

    public BaseRowsInput(string fullRepositoryName, string? token = null) : this(token)
    {
        (Owner, Repository) = SplitFullRepositoryName(fullRepositoryName);
    }

    public BaseRowsInput(string? token = null)
    {
        Client = new GitHubClient(new ProductHeaderValue(QueryCatApplication.ProductName));
        if (!string.IsNullOrEmpty(token))
        {
            Client.Credentials = new Credentials(token);
        }
    }

    private static (string Owner, string Repository) SplitFullRepositoryName(string fullRepositoryName)
    {
        var arr = fullRepositoryName.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (arr.Length != 2)
        {
            throw new QueryCatException($"Invalid {fullRepositoryName} format.");
        }
        return (arr[0], arr[1]);
    }
}
