using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

public class InterestCategoryModel
{
    /// <summary>
    /// Unique identifier for the interest category.
    /// </summary>
    [Description("Unique identifier for the interest category.")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Interests.
    /// </summary>
    public List<InterestModel> Interests { get; set; } = [];
}
