using System.Text.Json.Nodes;
using QueryCat.Backend;
using RestSharp;

namespace QueryCat.Plugins.Jira.Utils;

/// <summary>
/// Extensions for <see cref="RestResponse" />.
/// </summary>
internal static class RestResponseExtensions
{
    /// <summary>
    /// Converts JSON response into <see cref="JsonNode" />.
    /// </summary>
    /// <param name="response">RestSharp response.</param>
    /// <returns>Instance of <see cref="JsonNode" />.</returns>
    internal static JsonNode ToJson(this RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
        {
            throw new QueryCatException("Empty response.");
        }

        var json = JsonNode.Parse(response.Content);
        if (json == null)
        {
            throw new QueryCatException("Empty response.");
        }

        if (!response.IsSuccessful)
        {
            throw new QueryCatException($"Error query data: status={response.StatusCode}");
        }

        return json;
    }
}
