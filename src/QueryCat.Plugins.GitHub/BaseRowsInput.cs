using Octokit;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Fetch;

namespace QueryCat.Plugins.Github;

internal abstract class BaseRowsInput<TClass> : FetchRowsInput<TClass> where TClass : class
{
    protected GitHubClient Client { get; }

    public BaseRowsInput(string? token = null)
    {
        Client = new GitHubClient(new ProductHeaderValue(Backend.Core.Application.ProductName));
        if (!string.IsNullOrEmpty(token))
        {
            Client.Credentials = new Credentials(token);
        }
    }

    protected static (string Owner, string Repository) SplitFullRepositoryName(string fullRepositoryName)
    {
        var arr = fullRepositoryName.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (arr.Length != 2)
        {
            throw new QueryCatException($"Invalid '{fullRepositoryName}' format.");
        }
        return (arr[0], arr[1]);
    }

    protected static string GetFullRepositoryName(string owner, string repository)
        => $"{owner}/{repository}";
}
