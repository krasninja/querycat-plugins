using RestSharp;

namespace QueryCat.Plugins.ImdbApi.Utils;

/// <summary>
/// Extensions for <see cref="RestRequest" />.
/// </summary>
public static class RestRequestExtensions
{
    public static string Dump(this RestRequest request, RestClient client)
    {
        return string.Concat(
            request.Method.ToString().ToUpperInvariant(),
            ' ',
            client.BuildUri(request));
    }
}
