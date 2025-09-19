using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryCat.Plugins.ImdbApi;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Models.TitleModel))]
[JsonSerializable(typeof(IList<Models.TitleModel>))]
[JsonSerializable(typeof(Models.RatingModel))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(IList<Models.EpisodeModel>))]
[JsonSerializable(typeof(Models.EpisodeModel))]
[JsonSerializable(typeof(Models.PrecisionDateModel))]
[JsonSerializable(typeof(Models.InterestCategoryModel))]
[JsonSerializable(typeof(IList<Models.InterestCategoryModel>))]
[JsonSerializable(typeof(Models.InterestModel))]
[JsonSerializable(typeof(IList<Models.InterestModel>))]
[JsonSerializable(typeof(Models.NameModel))]
internal partial class SourceGenerationContext : JsonSerializerContext;
