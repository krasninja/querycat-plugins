using System.Text.Json.Serialization;

namespace QueryCat.Plugins.ImdbApi;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Models.TitleModel))]
[JsonSerializable(typeof(Models.RatingModel))]
internal partial class SourceGenerationContext : JsonSerializerContext;
