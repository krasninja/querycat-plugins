using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using QueryCat.Backend.Core.Execution;
using QChatMessage = QueryCat.Backend.Core.Execution.ChatMessage;
using QChatRequest = QueryCat.Backend.Core.Execution.ChatRequest;
using QChatResponse = QueryCat.Backend.Core.Execution.ChatResponse;

namespace QueryCat.Plugins.OpenAI;

public sealed class OpenAiAnswerAgent : IAnswerAgent
{
    private readonly ChatClient _chatClient;

    public OpenAiAnswerAgent(string key, string model = "gpt-4o-mini")
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(key),
            new OpenAIClientOptions
            {
                UserAgentApplicationId = QueryCat.Backend.Core.Application.ProductName,
            });
        _chatClient = client.GetChatClient(model);
    }

    private static global::OpenAI.Chat.ChatMessage CreateMessage(QChatMessage message)
    {
        switch (message.Role)
        {
            case QChatMessage.RoleSystem:
                return new SystemChatMessage(message.Content);
            case QChatMessage.RoleUser:
                return new UserChatMessage(message.Content);
            case QChatMessage.RoleAssistant:
                return new AssistantChatMessage(message.Content);
            case QChatMessage.RoleTool:
                return new ToolChatMessage(message.Content);
            default:
                return new UserChatMessage(message.Content);
        }
    }

    /// <inheritdoc />
    public async Task<QChatResponse> AskAsync(QChatRequest request, CancellationToken cancellationToken = default)
    {
        var messages = request.Messages.Select(CreateMessage);
        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var content = string.Join('\n', response.Value.Content.Select(c => c.Text.Trim()));
        return new QChatResponse(content, response.Value.Id);
    }
}
