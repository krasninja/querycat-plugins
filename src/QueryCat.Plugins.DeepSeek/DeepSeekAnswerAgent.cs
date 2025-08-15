using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;

namespace QueryCat.Plugins.DeepSeek;

internal sealed class DeepSeekAnswerAgent : IAnswerAgent, IDisposable
{
    private const string CompletionsUrl = @"https://api.deepseek.com/chat/completions";

    private readonly string _apiKey;
    private readonly string _model;
    private readonly HttpClient _httpClient;

    internal sealed class CompletionsModel
    {
        public string Model { get; set; } = "deepseek-chat";

        public QuestionMessage[] Messages { get; set; } = [];

        public bool Stream { get; set; }
    }

    internal sealed class InvalidResponseModel
    {
        public InvalidResponseErrorModel Error { get; set; } = new();
    }

    internal sealed class InvalidResponseErrorModel
    {
        public string Message { get; set; } = "Invalid request.";
    }

    internal sealed class ResponseModel
    {
        public string Id { get; set; } = string.Empty;

        public List<ResponseChoiceModel> Choices { get; set; } = new();
    }

    internal sealed class ResponseChoiceModel
    {
        public int Index { get; set; }

        public QuestionMessage Message { get; set; } = new(string.Empty);
    }

    public DeepSeekAnswerAgent(string apiKey, string model = "deepseek-chat")
    {
        _apiKey = apiKey;
        _model = model;
        _httpClient = new HttpClient();
    }

    /// <inheritdoc />
    public async Task<QuestionResponse> AskAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        requestMessage.Headers.Add("Accept", "application/json");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        var json = JsonSerializer.Serialize(new CompletionsModel
        {
            Model = _model,
            Messages = request.Messages,
        }, SourceGenerationContext.Default.CompletionsModel);
        requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var errorModel = JsonSerializer.Deserialize<InvalidResponseModel>(errorContent, SourceGenerationContext.Default.InvalidResponseModel);
            if (errorModel == null)
            {
                errorModel = new();
            }
            throw new QueryCatException(errorModel.Error.Message);
        }

        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        var promptResponse = JsonSerializer.Deserialize<ResponseModel>(jsonString, SourceGenerationContext.Default.ResponseModel);
        if (promptResponse == null || promptResponse.Choices.Count < 1)
        {
            throw new QueryCatException("Invalid request.");
        }

        var message = promptResponse.Choices[0].Message.Content;
        return new QuestionResponse(message);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
