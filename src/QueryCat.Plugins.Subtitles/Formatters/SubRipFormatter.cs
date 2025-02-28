using System.ComponentModel;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.Subtitles.Inputs;

namespace QueryCat.Plugins.Subtitles.Formatters;

/// <summary>
/// Formatter for SRT (SubRip) format.
/// </summary>
internal sealed class SubRipFormatter : IRowsFormatter
{
    [SafeFunction]
    [Description("SubRip (SRT) formatter.")]
    [FunctionSignature("srt(): object<IRowsFormatter>")]
    [FunctionFormatters(".srt", "application/x-subrip")]
    public static VariantValue Srt(IExecutionThread thread)
    {
        var rowsSource = new SubRipFormatter();
        return VariantValue.CreateFromObject(rowsSource);
    }

    /// <inheritdoc />
    public IRowsInput OpenInput(IBlobData blob, string? key = null)
        => new SubRipInput(new StreamReader(blob.GetStream()), keys: key ?? string.Empty);

    /// <inheritdoc />
    public IRowsOutput OpenOutput(IBlobData blob) => throw new NotImplementedException();
}
