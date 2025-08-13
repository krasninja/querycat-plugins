using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using QueryCat.Backend.Core.Execution;

namespace QueryCat.Plugins.Ollama;

internal sealed class OllamaAnswerAgent : IAnswerAgent, IDisposable
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
    public async Task<QuestionResponse> AskAsync(QuestionRequest request, CancellationToken cancellationToken = default)
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

        return new QuestionResponse(sb.ToString());
    }

    private static ChatRole ConvertChatRole(string role)
    {
        return role switch
        {
            QuestionMessage.RoleSystem => ChatRole.System,
            QuestionMessage.RoleAssistant => ChatRole.Assistant,
            QuestionMessage.RoleTool => ChatRole.Tool,
            _ => ChatRole.User,
        };
    }

    public void Dispose()
    {
        _client.Dispose();
        _httpClient.Dispose();
    }
}
