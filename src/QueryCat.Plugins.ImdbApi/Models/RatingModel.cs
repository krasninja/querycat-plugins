using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

/// <summary>
/// The Rating message represents the aggregate rating and votes count for a title.
/// </summary>
public class RatingModel
{
    /// <summary>
    /// The aggregate_rating field contains the average rating of the title,
    /// typically on a scale from 1 to 10.
    /// </summary>
    [Description("The aggregate_rating field contains the average rating of the title, typically on a scale from 1 to 10.")]
    public decimal AggregateRating { get; set; }

    /// <summary>
    /// The votes_count field contains the total number of votes cast for the title.
    /// </summary>
    [Description("The votes_count field contains the total number of votes cast for the title.")]
    public int VoteCount { get; set; }
}
