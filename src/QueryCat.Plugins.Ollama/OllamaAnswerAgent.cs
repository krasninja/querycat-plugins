using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Chat;
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
        _httpClient.Timeout = TimeSpan.FromHours(1);
        _httpClient.BaseAddress = new Uri(uri);
        _client = new OllamaApiClient(_httpClient);
        _client.SelectedModel = model;
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
        await foreach (var chat in _client.ChatAsync(chatRequest, cancellationToken))
        {
            if (chat == null || string.IsNullOrEmpty(chat.Message.Content))
            {
                continue;
            }
            sb.Append(chat.Message.Content);
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

    public void Dispose()
    {
        _client.Dispose();
        _httpClient.Dispose();
    }
}
