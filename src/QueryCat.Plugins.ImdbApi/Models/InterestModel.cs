using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

public class InterestModel
{
    /// <summary>
    /// Unique identifier for the interest.
    /// </summary>
    [Description("Unique identifier for the interest.")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the interest.
    /// </summary>
    [Description("The name of the interest.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the interest, which can include details about the genre or type.
    /// </summary>
    [Description("A brief description of the interest, which can include details about the genre or type.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the interest is a subgenre of another genre.
    /// </summary>
    [Description("Indicates whether the interest is a subgenre of another genre.")]
    public bool IsSubgenre { get; set; }

    /// <summary>
    /// Category of the interest.
    /// </summary>
    [Description("Category of the interest.")]
    public string Category { get; set; } = string.Empty;
}
