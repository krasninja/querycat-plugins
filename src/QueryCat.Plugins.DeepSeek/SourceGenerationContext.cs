using System.Text.Json.Serialization;

namespace QueryCat.Plugins.DeepSeek;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DeepSeekAnswerAgent.CompletionsModel))]
[JsonSerializable(typeof(DeepSeekAnswerAgent.InvalidResponseErrorModel))]
[JsonSerializable(typeof(DeepSeekAnswerAgent.InvalidResponseModel))]
[JsonSerializable(typeof(DeepSeekAnswerAgent.ResponseChoiceModel))]
[JsonSerializable(typeof(DeepSeekAnswerAgent.ResponseModel))]
internal partial class SourceGenerationContext : JsonSerializerContext;
