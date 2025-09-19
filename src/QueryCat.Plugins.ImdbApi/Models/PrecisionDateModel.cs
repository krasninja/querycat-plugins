using System.ComponentModel;

namespace QueryCat.Plugins.ImdbApi.Models;

/// <summary>
/// The PrecisionDate message represents a specific date,
/// typically used for birth dates, death dates, or release dates.
/// </summary>
public class PrecisionDateModel
{
    /// <summary>
    /// The year of the date, represented as an integer.
    /// </summary>
    [Description("The year of the date, represented as an integer.")]
    public int Year { get; set; }

    /// <summary>
    /// The month of the date, represented as an integer.
    /// </summary>
    [Description("The month of the date, represented as an integer.")]
    public int Month { get; set; }

    /// <summary>
    /// The day of the date, represented as an integer.
    /// </summary>
    [Description("The day of the date, represented as an integer.")]
    public int Day { get; set; }

    public DateOnly? ToDate() => Year > 0 ? new(Year, Month, Day) : null;
}
