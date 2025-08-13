using System.Text.Json.Serialization;

namespace QueryCat.Plugins.GigaChat;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GigaChatAnswerAgent.PromptRequestModel))]
[JsonSerializable(typeof(GigaChatAnswerAgent.PromptResponseModel))]
[JsonSerializable(typeof(GigaChatAnswerAgent.PromptResponseChoiceModel))]
[JsonSerializable(typeof(GigaChatAnswerAgent.PromptResponseChoiceMessageModel))]
internal partial class SourceGenerationContext : JsonSerializerContext;
