using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using QueryCat.Backend.Core.Execution;

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

    private static ChatMessage CreateMessage(QuestionMessage message)
    {
        switch (message.Role)
        {
            case QuestionMessage.RoleSystem:
                return new SystemChatMessage(message.Content);
            case QuestionMessage.RoleUser:
                return new UserChatMessage(message.Content);
            case QuestionMessage.RoleAssistant:
                return new AssistantChatMessage(message.Content);
            case QuestionMessage.RoleTool:
                return new ToolChatMessage(message.Content);
            default:
                return new UserChatMessage(message.Content);
        }
    }

    /// <inheritdoc />
    public async Task<QuestionResponse> AskAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var messages = request.Messages.Select(CreateMessage);
        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var content = string.Join('\n', response.Value.Content.Select(c => c.Text.Trim()));
        return new QuestionResponse(content, response.Value.Id);
    }
}
