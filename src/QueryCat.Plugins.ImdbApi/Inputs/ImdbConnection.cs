using System.Text.Json;
using RestSharp;

namespace QueryCat.Plugins.ImdbApi.Inputs;

internal static class ImdbConnection
{
    public static RestClient Client { get; } = new("https://api.imdbapi.dev/");

    public static string GetNextPageToken(JsonElement jsonElement)
    {
        if (jsonElement.TryGetProperty("nextPageToken", out var nextPageTokenJsonElement))
        {
            return nextPageTokenJsonElement.GetString() ?? string.Empty;
        }
        return string.Empty;
    }
}
