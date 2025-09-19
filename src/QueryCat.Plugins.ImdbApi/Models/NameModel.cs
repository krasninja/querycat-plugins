using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

public class NameModel
{
    /// <summary>
    /// The unique identifier for the name in the IMDb database.
    /// </summary>
    [Description("The unique identifier for the name in the IMDb database.")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the person, typically their full name.
    /// </summary>
    [Description("The display name of the person, typically their full name.")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Alternative names for the person, which may include stage names,
    /// nicknames, or other variations.
    /// </summary>
    [Description("Alternative names for the person, which may include stage names,\nnicknames, or other variations.")]
    public List<string> AlternativeNames { get; set; } = new();

    /// <summary>
    /// A brief biography or description of the person, which may include
    /// their career highlights, achievements, and other relevant information.
    /// </summary>
    [Description("A brief biography or description of the person, which may include their career highlights, achievements, and other relevant information.")]
    public string Biography { get; set; } = string.Empty;

    /// <summary>
    /// The height of the person in centimeters.
    /// </summary>
    [Description("The height of the person in centimeters.")]
    public int? HeightCm { get; set; }

    /// <summary>
    /// The birth name of the person, which may differ from their display name.
    /// </summary>
    [Description("The birth name of the person, which may differ from their display name.")]
    public string BirthName { get; set; } = string.Empty;

    /// <summary>
    /// The birth name of the person, which may differ from their display name.
    /// </summary>
    public PrecisionDateModel BirthDate { get; set; } = new();

    /// <summary>
    /// The birth location of the person, which may include the city and country of birth.
    /// </summary>
    [Description("The birth location of the person, which may include the city and country of birth.")]
    public string BirthLocation { get; set; } = string.Empty;

    /// <summary>
    /// The death date of the person.
    /// </summary>
    public PrecisionDateModel DeathDate { get; set; } = new();

    /// <summary>
    /// The death location of the person, which may include the city and country of death.
    /// </summary>
    [Description("The death location of the person, which may include the city and country of death.")]
    public string DeathLocation { get; set; } = string.Empty;

    /// <summary>
    /// The reason for the person's death, if applicable.
    /// </summary>
    [Description("The reason for the person's death, if applicable.")]
    public string DeathReason { get; set; } = string.Empty;
}
