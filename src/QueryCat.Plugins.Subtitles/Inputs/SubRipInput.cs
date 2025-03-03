using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Subtitles.Inputs;

internal sealed class SubRipInput : RowsInput
{
    private const string TimeFormat = "hh\\:mm\\:ss\\,fff";

    private readonly StreamReader _streamReader;

    private int _counter = 0;
    private TimeSpan _start = TimeSpan.Zero;
    private TimeSpan _end = TimeSpan.Zero;
    private string _text = string.Empty;

    private static readonly Regex _timesRegex = new(@"(?<start>[0-9,:]+)\s+-->\s+(?<end>[0-9,:]+)", RegexOptions.Compiled);

    /// <inheritdoc />
    public override Column[] Columns { get; protected set; } =
    [
        new("counter", DataType.Integer, "A numeric counter identifying each sequential subtitle."),
        new("start_time", DataType.Timestamp, "The start time that the subtitle should appear on the screen."),
        new("end_time", DataType.Timestamp, "The end time that the subtitle should disappear from the screen."),
        new("text", DataType.String, "Subtitle text.")
    ];

    /// <inheritdoc />
    public SubRipInput(StreamReader streamReader, params string[] keys)
    {
        _streamReader = streamReader;
        UniqueKey = keys;
    }

    /// <inheritdoc />
    public override Task OpenAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task CloseAsync(CancellationToken cancellationToken = default)
    {
        _streamReader.Close();
        _streamReader.Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override ErrorCode ReadValue(int columnIndex, out VariantValue value)
    {
        if (columnIndex == 0)
        {
            value = new VariantValue(_counter);
            return ErrorCode.OK;
        }
        if (columnIndex == 1)
        {
            value = new VariantValue(_start);
            return ErrorCode.OK;
        }
        if (columnIndex == 2)
        {
            value = new VariantValue(_end);
            return ErrorCode.OK;
        }
        if (columnIndex == 3)
        {
            value = new VariantValue(_text);
            return ErrorCode.OK;
        }

        throw new ArgumentOutOfRangeException(nameof(columnIndex));
    }

    /// <inheritdoc />
    public override async ValueTask<bool> ReadNextAsync(CancellationToken cancellationToken = default)
    {
        _counter = 0;
        _start = TimeSpan.Zero;
        _end = TimeSpan.Zero;
        _text = string.Empty;

        // Counter.
        var line = string.Empty;
        while (string.IsNullOrEmpty(line))
        {
            if (_streamReader.EndOfStream)
            {
                return false;
            }
            line = await _streamReader.ReadLineAsync(cancellationToken);
        }
        _counter = int.Parse(line);

        // Time.
        line = await _streamReader.ReadLineAsync(cancellationToken);
        if (line == null)
        {
            return false;
        }
        var match = _timesRegex.Match(line);
        if (match.Success)
        {
            _start = TimeSpan.ParseExact(match.Groups["start"].Value, TimeFormat, formatProvider: null, TimeSpanStyles.None);
            _end = TimeSpan.ParseExact(match.Groups["end"].Value, TimeFormat, formatProvider: null, TimeSpanStyles.None);
        }

        // Text.
        var sb = new StringBuilder(0);
        while (!string.IsNullOrEmpty(line = await _streamReader.ReadLineAsync(cancellationToken)))
        {
            sb.AppendLine(line);
        }
        _text = new VariantValue(sb.ToString().Trim());

        return true;
    }
}
