using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using SharpPcap;
using SharpPcap.LibPcap;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.PostgresSniffer.Utils;

namespace QueryCat.Plugins.PostgresSniffer.Inputs;

/// <summary>
/// Postgres simple protocol sniffer.
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/protocol-message-formats.html.
/// </remarks>
internal sealed class PostgresQueriesRowsInput : IRowsInput, IDisposable
{
    private const int MaxMessageSize = 1_048_576;

    private readonly string _iface;
    private readonly string _host;
    private readonly ushort _port;

    private ILiveDevice? _device;
    private readonly TcpSplitter _tcpSplitter = new();
    private TcpSplitter.SessionBuffer? _currentBuffer;
    private readonly byte[] _postgresPacketHead = new byte[5];

    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(nameof(PostgresQueriesRowsInput));

    [Description("Listen TCP traffic for the specified host and port.")]
    [FunctionSignature("pgsniff_start(iface?: string := null, host?: string := null, port: integer := 5432): object<IRowsInput>")]
    public static VariantValue PostgresSnifferStart(FunctionCallInfo args)
    {
        var iface = args.GetAt(0);
        var host = args.GetAt(1);
        var port = args.GetAt(2);
        return VariantValue.CreateFromObject(
            new PostgresQueriesRowsInput(iface.AsString, host.AsString, (ushort)port.AsInteger));
    }

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    /// <inheritdoc />
    public Column[] Columns { get; } =
    [
        new Column("destination_ip", DataType.String, "Destination IP address."),
        new Column("source_ip", DataType.String, "Source IP address."),
        new Column("destination_port", DataType.Integer, "Destination port."),
        new Column("source_port", DataType.Integer, "Source port."),
        new Column("type", DataType.String, "Message type."),
        new Column("length", DataType.Integer, "Message length."),
        new Column("query", DataType.String, "SQL text.")
    ];

    /// <inheritdoc />
    public string[] UniqueKey { get; }

    public PostgresQueriesRowsInput(string iface, string host, ushort port)
    {
        _iface = iface;
        _host = host;
        _port = port;

        UniqueKey = [_iface, _host, _port.ToString()];
    }

    /// <inheritdoc />
    public void Open()
    {
        if (!string.IsNullOrEmpty(_iface))
        {
            foreach (var device in CaptureDeviceList.Instance)
            {
                if (string.Equals(device.Name, _iface, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(device.Description, _iface, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(device.MacAddress?.ToString(), _iface, StringComparison.OrdinalIgnoreCase)
                    || IsMatchAddress(device, _iface))
                {
                    _device = device;
                    break;
                }
            }
        }
        else
        {
            _device = CaptureDeviceList.Instance.FirstOrDefault(d => d.Name != "lo");
        }

        if (_device == null)
        {
            throw new QueryCatException(
                $"Cannot get device by name '{_iface}'. Please provide device name, IP or MAC address.");
        }

        _device.OnPacketArrival += DeviceOnOnPacketArrival;
        _device.Open();
        _device.Filter = GetFilter();
        _logger.LogDebug("Filter '{Filter}'.", _device.Filter);
        _device.StartCapture();
    }

    private void DeviceOnOnPacketArrival(object sender, PacketCapture e)
    {
        _tcpSplitter.CapturePacket(e);
    }

    private bool IsMatchAddress(ILiveDevice device, string address)
    {
        var match = false;
        if (!match && device is LibPcapLiveDevice libPcapLiveDevice)
        {
            match = libPcapLiveDevice.Addresses.Any(a =>
                a.Addr.ipAddress != null &&
                string.Equals(a.Addr.ipAddress.ToString(), address, StringComparison.OrdinalIgnoreCase));
            if (!match)
            {
                match = libPcapLiveDevice.Addresses.Any(a =>
                    a.Addr.hardwareAddress != null &&
                    string.Equals(a.Addr.hardwareAddress.ToString(), address, StringComparison.OrdinalIgnoreCase));
            }
        }
        return match;
    }

    private string GetFilter()
    {
        // https://www.tcpdump.org/manpages/pcap-filter.7.html
        var filters = new List<string>();
        if (!string.IsNullOrEmpty(_host))
        {
            filters.Add("host " + _host);
        }
        filters.Add("tcp dst port " + _port);
        return string.Join(" and ", filters);
    }

    /// <inheritdoc />
    public void Close()
    {
        if (_device != null)
        {
            _device.StopCapture();
            _device.Close();
            _device = null;
        }
        _tcpSplitter.Dispose();
    }

    /// <inheritdoc />
    public void Reset()
    {
        Close();
        Open();
    }

    /// <inheritdoc />
    public ErrorCode ReadValue(int columnIndex, out VariantValue value)
    {
        value = VariantValue.Null;
        if (_currentBuffer == null || _currentBuffer.Size < 5)
        {
            return ErrorCode.Error;
        }
        if (!_currentBuffer.TryCopyExact(_postgresPacketHead, advance: false))
        {
            return ErrorCode.Error;
        }

        // destination_ip
        if (columnIndex == 0)
        {
            value = new VariantValue(_currentBuffer.Session.DestinationAddress.ToString());
        }
        // source_ip
        else if (columnIndex == 1)
        {
            value = new VariantValue(_currentBuffer.Session.SourceAddress.ToString());
        }
        // destination_port
        else if (columnIndex == 2)
        {
            value = new VariantValue(_currentBuffer.Session.DestinationPort);
        }
        // source_port
        else if (columnIndex == 3)
        {
            value = new VariantValue(_currentBuffer.Session.SourcePort);
        }
        // type
        else if (columnIndex == 4)
        {
            value = new VariantValue(Convert.ToChar(_postgresPacketHead[0]));
        }
        // length
        else if (columnIndex == 5)
        {
            value = new VariantValue(GetTotalLengthFromPacketHead() - 1);
        }
        // query
        else if (columnIndex == 6)
        {
            using var packetMemory = MemoryPool<byte>.Shared.Rent(GetTotalLengthFromPacketHead());
            value = new VariantValue(ReadQuery(packetMemory.Memory.Span));
        }

        return ErrorCode.OK;
    }

    private string ReadQuery(Span<byte> buf)
    {
        if (_currentBuffer == null)
        {
            return string.Empty;
        }

        var packetSize = GetTotalLengthFromPacketHead();
        var packetSpan = buf[..packetSize];
        if (!_currentBuffer.TryCopyExact(packetSpan, advance: false))
        {
            _logger.LogWarning("Cannot copy packet! Packet size: {HeadSize}, buffer size: {BufferSize}.",
                packetSpan.Length, _currentBuffer.Size);
            return string.Empty;
        }

        // Seems packet without data.
        if (packetSpan.Length < 6)
        {
            return string.Empty;
        }

        // Skip head.
        packetSpan = packetSpan[5..];

        // P - prepare.
        if (_postgresPacketHead[0] == 80)
        {
            var prepareName = ReadNullTerminatedString(packetSpan);
            var nextIndex = prepareName.Length + 1;

            if (packetSpan.Length > nextIndex)
            {
                return ReadNullTerminatedString(packetSpan[nextIndex..]);
            }
        }
        // Q - query.
        else if (_postgresPacketHead[0] == 81)
        {
            return ReadNullTerminatedString(packetSpan);
        }

        return string.Empty;
    }

    private static readonly byte[] _zeroByte = [0];

    private static string ReadNullTerminatedString(ReadOnlySpan<byte> bytes)
    {
        var zeroIndex = bytes.IndexOf(_zeroByte);
        return zeroIndex > -1
            ? System.Text.Encoding.Default.GetString(bytes[..zeroIndex])
            : System.Text.Encoding.Default.GetString(bytes);
    }

    /// <inheritdoc />
    public bool ReadNext()
    {
        if (_device == null)
        {
            return false;
        }

        if (_currentBuffer != null)
        {
            var postgresPacketSize = GetTotalLengthFromPacketHead();
            _currentBuffer.Advance(postgresPacketSize);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Advance on {Size}, Size after {SizeAfter}.", postgresPacketSize, _currentBuffer.Size);
            }
            Array.Fill<byte>(_postgresPacketHead, 0);
            _currentBuffer = null;
        }

        foreach (var sessionBuffer in _tcpSplitter.GetSessionBuffers())
        {
            if (sessionBuffer.TryCopyExact(_postgresPacketHead, advance: false))
            {
                var postgresPacketSize = GetTotalLengthFromPacketHead();

                // Validate.
                var messageType = Convert.ToChar(_postgresPacketHead[0]);
                if ((!char.IsLetter(messageType) && !char.IsDigit(messageType))
                    || postgresPacketSize > MaxMessageSize)
                {
                    _logger.LogWarning("Invalid packet header!");
                    sessionBuffer.AdvanceToEnd();
                    continue;
                }

                var bufferSize = sessionBuffer.Size;

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Packet Size: {HeadSize}, Buffer size: {BufferSize}.", postgresPacketSize, bufferSize);
                }
                if (postgresPacketSize <= bufferSize)
                {
                    _currentBuffer = sessionBuffer;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Get total packet size including byte command (+1).
    /// </summary>
    private int GetTotalLengthFromPacketHead()
        => BinaryPrimitives.ReadInt32BigEndian(_postgresPacketHead[1..5]) + 1;

    /// <inheritdoc />
    public void Explain(IndentedStringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("pgsniffer");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Close();
    }
}
