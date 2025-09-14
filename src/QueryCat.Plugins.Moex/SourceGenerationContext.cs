using System.Text.Json.Serialization;
using QueryCat.Plugins.Moex.Inputs;

namespace QueryCat.Plugins.Moex;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(MoexInput.MoexDataModel))]
[JsonSerializable(typeof(Dictionary<string, MoexInput.MoexType>))]
[JsonSerializable(typeof(MoexInput.MoexType))]
[JsonSerializable(typeof(MoexInput.MoexCursor))]
internal partial class SourceGenerationContext : JsonSerializerContext;
