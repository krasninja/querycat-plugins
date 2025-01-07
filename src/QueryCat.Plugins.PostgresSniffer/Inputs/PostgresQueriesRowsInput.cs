using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using SharpPcap;
using SharpPcap.LibPcap;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Backend.Core.Utils;
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

    /// <summary>
    /// Current message data.
    /// </summary>
    private sealed class CurrentMessage
    {
        public TcpSplitter.SessionBuffer? CurrentBuffer { get; private set; }

        public char Type { get; private set; }

        /// <summary>
        /// Total length with message type byte.
        /// </summary>
        public int Length { get; private set; }

        public bool IsInitialized => CurrentBuffer != null;

        public bool IsValid => (char.IsLetter(Type) || char.IsDigit(Type)) && Length < MaxMessageSize;

        public void Clear()
        {
            Length = 0;
            Type = '\0';
            CurrentBuffer = null;
        }

        public bool Set(byte[] head, TcpSplitter.SessionBuffer buffer)
        {
            if (head.Length != 5)
            {
                return false;
            }
            CurrentBuffer = buffer;
            Length = BinaryPrimitives.ReadInt32BigEndian(head[1..5]) + 1;
            Type = Convert.ToChar(head[0]);
            return true;
        }
    }

    private readonly string _iface;
    private readonly string _host;
    private readonly ushort _port;

    private ILiveDevice? _device;
    private readonly TcpSplitter _tcpSplitter = new();
    private readonly byte[] _postgresPacketHead = new byte[5];
    private readonly CurrentMessage _currentMessage = new();

    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(nameof(PostgresQueriesRowsInput));

    [Description("Listen TCP traffic for the specified host and port.")]
    [FunctionSignature("pgsniff_start(iface?: string := null, host?: string := null, port: integer := 5432): object<IRowsInput>")]
    public static VariantValue PostgresSnifferStart(IExecutionThread thread)
    {
        var iface = thread.Stack[0];
        var host = thread.Stack[1];
        var port = thread.Stack[2];
        return VariantValue.CreateFromObject(
            new PostgresQueriesRowsInput(iface.AsString, host.AsString, port.ToUInt16()));
    }

    /// <inheritdoc />
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    /// <inheritdoc />
    public Column[] Columns { get; } =
    [
        new("destination_ip", DataType.String, "Destination IP address."),
        new("source_ip", DataType.String, "Source IP address."),
        new("destination_port", DataType.Integer, "Destination port."),
        new("source_port", DataType.Integer, "Source port."),
        new("type", DataType.String, "Message type."),
        new("length", DataType.Integer, "Message length."),
        new("query", DataType.String, "SQL text.")
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
    public Task OpenAsync(CancellationToken cancellationToken = default)
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
            // First device ignore loopback: "lo" or "\Device\NPF_Loopback".
            _device = CaptureDeviceList.Instance.FirstOrDefault(d => d.Name != "lo" && !d.Name.EndsWith("Loopback"));
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

        return Task.CompletedTask;
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
    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await CloseAsync(cancellationToken);
        await OpenAsync(cancellationToken);
    }

    /// <inheritdoc />
    public ErrorCode ReadValue(int columnIndex, out VariantValue value)
    {
        value = VariantValue.Null;
        var sessionBuffer = _currentMessage.CurrentBuffer;
        if (sessionBuffer == null || sessionBuffer.Size < _postgresPacketHead.Length)
        {
            return ErrorCode.Error;
        }

        // destination_ip
        if (columnIndex == 0)
        {
            value = new VariantValue(sessionBuffer.Session.DestinationAddress.ToString());
        }
        // source_ip
        else if (columnIndex == 1)
        {
            value = new VariantValue(sessionBuffer.Session.SourceAddress.ToString());
        }
        // destination_port
        else if (columnIndex == 2)
        {
            value = new VariantValue(sessionBuffer.Session.DestinationPort);
        }
        // source_port
        else if (columnIndex == 3)
        {
            value = new VariantValue(sessionBuffer.Session.SourcePort);
        }
        // type
        else if (columnIndex == 4)
        {
            value = new VariantValue(_currentMessage.Type);
        }
        // length
        else if (columnIndex == 5)
        {
            value = new VariantValue(_currentMessage.Length - 1);
        }
        // query
        else if (columnIndex == 6)
        {
            using var packetMemory = MemoryPool<byte>.Shared.Rent(_currentMessage.Length);
            value = new VariantValue(ReadQuery(packetMemory.Memory.Span));
        }

        return ErrorCode.OK;
    }

    private string ReadQuery(Span<byte> buf)
    {
        if (_currentMessage.CurrentBuffer == null)
        {
            return string.Empty;
        }

        var packetSpan = buf[.._currentMessage.Length];
        if (!_currentMessage.CurrentBuffer.TryCopyExact(packetSpan, advance: false))
        {
            _logger.LogWarning("Cannot copy packet! Packet size: {HeadSize}, buffer size: {BufferSize}.",
                packetSpan.Length, _currentMessage.CurrentBuffer.Size);
            return string.Empty;
        }

        // Seems packet without data.
        if (packetSpan.Length < 6)
        {
            return string.Empty;
        }

        // Skip head.
        packetSpan = packetSpan[_postgresPacketHead.Length..];

        // P - prepare.
        if (_currentMessage.Type == 'P')
        {
            var prepareName = ReadNullTerminatedString(packetSpan);
            var nextIndex = prepareName.Length + 1;
            if (packetSpan.Length > nextIndex)
            {
                return ReadNullTerminatedString(packetSpan[nextIndex..]);
            }
        }
        // Q - query.
        else if (_currentMessage.Type == 'Q')
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
    public ValueTask<bool> ReadNextAsync(CancellationToken cancellationToken = default)
    {
        if (_device == null)
        {
            return ValueTask.FromResult(false);
        }

        if (_currentMessage.IsInitialized && _currentMessage.CurrentBuffer != null)
        {
            _currentMessage.CurrentBuffer.Advance(_currentMessage.Length);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Advance on: {Size}, size after: {SizeAfter}.",
                    _currentMessage.Length, _currentMessage.CurrentBuffer.Size);
            }
            Array.Fill<byte>(_postgresPacketHead, 0);
            _currentMessage.Clear();
        }

        foreach (var sessionBuffer in _tcpSplitter.GetSessionBuffers())
        {
            if (sessionBuffer.TryCopyExact(_postgresPacketHead, advance: false))
            {
                _currentMessage.Set(_postgresPacketHead, sessionBuffer);

                // Validate.
                if (!_currentMessage.IsValid)
                {
                    _logger.LogDebug("Invalid packet header!");
                    sessionBuffer.AdvanceToEnd();
                    _currentMessage.Clear();
                    continue;
                }

                var bufferSize = sessionBuffer.Size;

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Packet size: {HeadSize}, buffer size: {BufferSize}.",
                        _currentMessage.Length, bufferSize);
                }
                if (_currentMessage.Length <= bufferSize)
                {
                    return ValueTask.FromResult(true);
                }
                _currentMessage.Clear();
            }
        }

        return ValueTask.FromResult(false);
    }

    /// <inheritdoc />
    public void Explain(IndentedStringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("pgsniffer");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        AsyncUtils.RunSync(CloseAsync);
        if (_device != null)
        {
            _device.StopCapture();
            _device.Close();
            _device.Dispose();
            _device = null;
        }
        _tcpSplitter.Dispose();
    }
}
