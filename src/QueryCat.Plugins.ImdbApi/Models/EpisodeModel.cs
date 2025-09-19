using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

/// <summary>
/// IMDb episode model.
/// </summary>
public class EpisodeModel
{
    /// <summary>
    /// The unique identifier for the episode.
    /// </summary>
    [Description("The unique identifier for the episode.")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title.
    /// </summary>
    [Description("Title.")]
    public string Title { get; set; } = string.Empty;

    public string Season { get; set; } = string.Empty;

    /// <summary>
    /// Episode number.
    /// </summary>
    [Description("Episode number.")]
    public int EpisodeNumber { get; set; }

    /// <summary>
    /// The runtime_seconds field contains the total runtime of the episode in minutes.
    /// </summary>
    [Description("The runtime_seconds field contains the total runtime of the episode in minutes.")]
    public int RuntimeSeconds { get; set; }

    /// <summary>
    /// The plot field contains a brief summary or description of the episode's storyline.
    /// </summary>
    [Description("The plot field contains a brief summary or description of the episode's storyline.")]
    public string Plot { get; set; } = string.Empty;

    /// <summary>
    /// The Rating message represents the aggregate rating and votes count for a title.
    /// </summary>
    public RatingModel Rating { get; set; } = new();

    public PrecisionDateModel Date { get; set; } = new();
}
