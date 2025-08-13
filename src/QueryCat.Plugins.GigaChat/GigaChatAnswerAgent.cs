using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;

namespace QueryCat.Plugins.GigaChat;

internal sealed class GigaChatAnswerAgent : IAnswerAgent, IDisposable
{
    private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    private const string CompletionsUrl = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

    private const string RootRussianPublicKey =
        """
        MIICCgKCAgEAx8U5nylQAvf6vaeqoTRmnnax6VewoYVigbQYzlvDPVtIW0K34BlA
        yGRZCF4jemhkBOhgm7r2kcspLpBcGLAELVy/NiZRgoxhkLuMTliERTZtIvSZfs1o
        zEwOYfb83C45VGPw4iZVrmzUXhTOfgq/c8WUMGONKNcpVj2SaNQGxdCsgd5qqZQi
        w8iU1ZSeKZdLQjRpsTGqRt2tdtdjAI5eE47akNTHdySYmUIxQZpxROfKXJBbZWwk
        jIgYDxXTHN1p5ReDRVnpmY1SvlgF6v8QA4s9vw1imwCEl7aZeMwH8n0c2ygUwEUn
        SUs5P/51C+Nt1Fmg5Px6omladUNT5Av+oRmfPns3zw5YzetpsmRE11T9nvHlIUgz
        0Wuq03zF7CyIFYEjQrpcW44E5MPhXTyjhPMnz4JyrleUJRbYvjylk0Ji4EN8GHsX
        GQHuoOAYOJp+0SRll8ClGDYT4z0bzCQ0pM8sNzjAfQUNOKOGDFHdjg+JLUcvZmHD
        tsPcJuyWYQaB+edmiM2Qm1wt4EcEtrnb91LA1ThZYu5tphKICYD0hQxfX9Gl+nE7
        F3hiSaHP3ugVtRoMkWKkiCDHmxd48CWRN1ae/5FYHGUnAxDbmgQeZGC41h/hmv9H
        Gv1xL3dj6Z1chloEQTQpLaItGpo6JYGSL0gxBTimGo84EBobsD54/w8CAwEAAQ==
        """;

    private readonly string _authKey;
    private readonly HttpClient _httpClient;

    private string _accessToken = string.Empty;
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(GigaChatAnswerAgent));

    private static readonly RSA _rootRussianPublicKey;

    static GigaChatAnswerAgent()
    {
        _rootRussianPublicKey = RSA.Create();
        _rootRussianPublicKey.ImportRSAPublicKey(Convert.FromBase64String(RootRussianPublicKey.Replace("\n", string.Empty)), out _);
    }

    internal sealed class PromptRequestModel
    {
        public string Model { get; set; } = "GigaChat";

        public QuestionMessage[] Messages { get; set; } = [];

        public bool Stream { get; set; }

        [JsonPropertyName("repetition_penalty")]
        public int RepetitionPenalty { get; set; } = 1;
    }

    internal sealed class PromptResponseModel
    {
        public PromptResponseChoiceModel[] Choices { get; set; } = [];

        public int Created { get; set; }

        public string Model { get; set; } = string.Empty;

        public string Object { get; set; } = string.Empty;
    }

    internal sealed class PromptResponseChoiceModel
    {
        public string FinishReason { get; set; } = string.Empty;

        public int Index { get; set; }

        public PromptResponseChoiceMessageModel Message { get; set; } = new();
    }

    internal sealed class PromptResponseChoiceMessageModel
    {
        public string Content { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }

    public GigaChatAnswerAgent(string authKey)
    {
        var handler = new HttpClientHandler();
        handler.UseDefaultCredentials = true;
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
        {
            if (chain == null)
            {
                return false;
            }

            var root = chain.ChainElements[^1];
            var rsaPublicKey = root.Certificate.PublicKey.GetRSAPublicKey();
            if (rsaPublicKey == null)
            {
                return false;
            }

            if (!rsaPublicKey.ExportRSAPublicKey().SequenceEqual(_rootRussianPublicKey.ExportRSAPublicKey()))
            {
                return false;
            }

            return true;
        };

        _authKey = authKey;
        _httpClient = new HttpClient(handler);
    }

    private PromptRequestModel ConvertRequest(QuestionRequest request)
    {
        var systemMessage = new QuestionMessage(
            string.Join('\n', request.Messages.Where(m => m.Role == QuestionMessage.RoleSystem).Select(m => m.Content)),
            QuestionMessage.RoleSystem);
        var userMessage = new QuestionMessage(
            string.Join('\n', request.Messages.Where(m => m.Role == QuestionMessage.RoleUser).Select(m => m.Content)),
            QuestionMessage.RoleUser);
        return new PromptRequestModel
        {
            Messages =
            [
                systemMessage,
                userMessage
            ]
        };
    }

    /// <inheritdoc />
    public async Task<QuestionResponse> AskAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var accessToken = await AuthorizeAsync(cancellationToken);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        PrepareHttpRequestMessage(requestMessage, accessToken);
        var json = JsonSerializer.Serialize(new PromptRequestModel
        {
            Messages = ConvertRequest(request).Messages,
        }, SourceGenerationContext.Default.PromptRequestModel);
        requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new QueryCatException($"Invalid request {response.StatusCode}: {errorContent}");
        }
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Server response: {Response}", jsonString);
        var promptResponse = JsonSerializer.Deserialize<PromptResponseModel>(jsonString,
            SourceGenerationContext.Default.PromptResponseModel);
        if (promptResponse == null || promptResponse.Choices.Length < 1)
        {
            throw new QueryCatException("Invalid request.");
        }
        var message = promptResponse.Choices[0].Message.Content;

        return new QuestionResponse(message);
    }

    private async Task<string> AuthorizeAsync(CancellationToken cancellationToken)
    {
        if (DateTimeOffset.UtcNow - _lastRefresh < TimeSpan.FromMinutes(10))
        {
            return _accessToken;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, AuthUrl);
        PrepareHttpRequestMessage(requestMessage);
        requestMessage.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
        ]);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authKey);
        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var json = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken);
        _accessToken = json.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        _lastRefresh = DateTimeOffset.Now;
        return _accessToken;
    }

    private void PrepareHttpRequestMessage(HttpRequestMessage message, string? accessToken = null)
    {
        message.Headers.Add("RqUID", Guid.NewGuid().ToString("D"));
        message.Headers.Add("Accept", "application/json");
        if (!string.IsNullOrEmpty(accessToken))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        message.Headers.Add("User-Agent", QueryCat.Backend.Core.Application.ProductName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
