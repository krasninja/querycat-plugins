using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

public class TitleModel
{
    /// <summary>
    /// The unique identifier for the title.
    /// </summary>
    [Description("The unique identifier for the title.")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of the title, such as "movie", "tvSeries", "tvEpisode", etc.
    /// </summary>
    [Description("The type of the title.")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The is_adult field indicates whether the title is intended for adult audiences.
    /// </summary>
    [Description("The is_adult field indicates whether the title is intended for adult audiences.")]
    public bool IsAdult { get; set; }

    /// <summary>
    /// The primary title of the title, which is typically the most recognized name.
    /// </summary>
    [Description("The primary title of the title, which is typically the most recognized name.")]
    public string PrimaryTitle { get; set; } = string.Empty;

    /// <summary>
    /// The original title of the title, normally which is the title as it was originally released.
    /// </summary>
    [Description("The original title of the title, normally which is the title as it was originally released.")]
    public string OriginalTitle { get; set; } = string.Empty;

    /// <summary>
    /// The runtime_seconds field contains the total runtime of the title in minutes.
    /// </summary>
    [Description("The runtime_seconds field contains the total runtime of the title in minutes.")]
    public int RuntimeSeconds { get; set; }

    /// <summary>
    /// The genres field contains a list of genres associated with the title.
    /// </summary>
    [Description("The genres field contains a list of genres associated with the title.")]
    public List<string> Genres { get; set; } = [];

    /// <summary>
    /// The start_year field is used for titles that have a defined start, such as movies or TV series.
    /// </summary>
    [Description("The start_year field is used for titles that have a defined start, such as movies or TV series.")]
    public int StartYear { get; set; }

    /// <summary>
    /// The end_year field is used for titles that have a defined end, such as TV series.
    /// </summary>
    [Description("The end_year field is used for titles that have a defined end, such as TV series.")]
    public int EndYear { get; set; }

    /// <summary>
    /// The Rating message represents the aggregate rating and votes count for a title.
    /// </summary>
    public RatingModel Rating { get; set; } = new();

    /// <summary>
    /// The plot field contains a brief summary or description of the title's storyline.
    /// </summary>
    [Description("The plot field contains a brief summary or description of the title's storyline.")]
    public string Plot { get; set; } = string.Empty;
}
