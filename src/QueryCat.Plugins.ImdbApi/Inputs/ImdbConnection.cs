using RestSharp;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal static class ImdbConnection
{
    public static RestClient Client { get; } = new("https://api.imdbapi.dev/");
}
