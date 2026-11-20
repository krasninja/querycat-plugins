using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using QueryCat.Backend.Core;
using QChatMessage = QueryCat.Backend.Core.Execution.ChatMessage;
using QChatRequest = QueryCat.Backend.Core.Execution.ChatRequest;
using QChatResponse = QueryCat.Backend.Core.Execution.ChatResponse;

namespace QueryCat.Plugins.Ollama;

internal sealed class OllamaAnswerAgent : QueryCat.Backend.Core.Execution.IAnswerAgent, IDisposable
{
    private readonly OllamaApiClient _client;
    private readonly HttpClient _httpClient = new();

    public OllamaAnswerAgent(string model, string uri)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new QueryCatException("Ollama model name cannot be empty.");
        }
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new QueryCatException($"Invalid Ollama URL: '{uri}'.");
        }
        // OllamaSharp uses relative URIs, so the base address must end with a slash
        // to preserve sub-paths when the server is behind a reverse proxy.
        if (!baseUri.AbsolutePath.EndsWith('/'))
        {
            baseUri = new Uri(baseUri, baseUri.AbsolutePath + "/");
        }
        _httpClient.Timeout = TimeSpan.FromHours(1);
        _httpClient.BaseAddress = baseUri;
        _client = new OllamaApiClient(_httpClient, model);
    }

    /// <inheritdoc />
    public async Task<QChatResponse> AskAsync(QChatRequest request, CancellationToken cancellationToken = default)
    {
        var chatRequest = new ChatRequest
        {
            Messages = request.Messages
                .Select(m => new Message(ConvertChatRole(m.Role), m.Content)),
            Think = false,
            KeepAlive = "1h",
        };

        var sb = new StringBuilder();
        try
        {
            await foreach (var chat in _client.ChatAsync(chatRequest, cancellationToken))
            {
                if (chat?.Message?.Content is { Length: > 0 } content)
                {
                    sb.Append(content);
                }
            }
        }
        catch (HttpRequestException e)
        {
            throw new QueryCatException($"Cannot access Ollama server at '{_httpClient.BaseAddress}': {e.Message}", e);
        }

        return new QChatResponse(sb.ToString());
    }

    private static ChatRole ConvertChatRole(string role)
    {
        return role switch
        {
            QChatMessage.RoleSystem => ChatRole.System,
            QChatMessage.RoleAssistant => ChatRole.Assistant,
            QChatMessage.RoleTool => ChatRole.Tool,
            _ => ChatRole.User,
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        _httpClient.Dispose();
    }
}
