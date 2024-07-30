using System.Buffers.Binary;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Backend.Core.Utils;

namespace QueryCat.Plugins.PostgresSniffer.Inputs;

/// <summary>
/// Postgres simple protocol sniffer.
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/protocol-message-formats.html.
/// </remarks>
internal sealed class PostgresQueriesRowsInput : IRowsInput
{
    private const int ReadTimeoutMilliseconds = 1000;

    private readonly string _iface;
    private readonly string _host;
    private readonly ushort _port;

    private ILiveDevice? _device;
    private TcpPacket? _tcpPacket;
    private readonly DynamicBuffer<byte> _buffer = new();

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

        _device.Open(DeviceModes.Promiscuous, ReadTimeoutMilliseconds);
        _device.Filter = GetFilter();
        _logger.LogDebug("Filter '{Filter}'.", _device.Filter);
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
            _device.Close();
            _device = null;
        }
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
        if (_tcpPacket == null || _buffer.Size < 1)
        {
            return ErrorCode.Error;
        }
        var ipPacket = _tcpPacket.ParentPacket as IPPacket;

        // destination_ip
        if (columnIndex == 0 && ipPacket != null)
        {
            value = new VariantValue(ipPacket.DestinationAddress.ToString());
        }
        // source_ip
        else if (columnIndex == 1 && ipPacket != null)
        {
            value = new VariantValue(ipPacket.SourceAddress.ToString());
        }
        // destination_port
        else if (columnIndex == 2 && ipPacket != null)
        {
            value = new VariantValue(_tcpPacket.DestinationPort);
        }
        // source_port
        else if (columnIndex == 3 && ipPacket != null)
        {
            value = new VariantValue(_tcpPacket.SourcePort);
        }
        // type
        else if (columnIndex == 4 && _buffer.Size > 0)
        {
            value = new VariantValue(Convert.ToChar(_buffer.GetAt(0)));
        }
        // length
        else if (columnIndex == 5 && _buffer.Size > 5)
        {
            value = new VariantValue(BinaryPrimitives.ReadInt32BigEndian(_buffer.GetSpan(1, 5)));
        }
        // query
        else if (columnIndex == 6 && _buffer.Size > 5)
        {
            // P - prepare.
            if (_buffer.GetAt(0) == 80)
            {
                var prepareName = ReadNullTerminatedString(_buffer.GetSpan(5));
                var nextIndex = 5 + prepareName.Length + 1;
                if (_buffer.Size > nextIndex)
                {
                    value = new VariantValue(ReadNullTerminatedString(_buffer.GetSpan(nextIndex)));
                }
            }
            // Q - query.
            else if (_buffer.GetAt(0) == 81)
            {
                value = new VariantValue(ReadNullTerminatedString(_buffer.GetSpan(5)));
            }
        }

        return ErrorCode.OK;
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
        _buffer.AdvanceToEnd();

        var isFirstPacket = true;
        var actualPacketLength = 0;
        do
        {
            // Read data.
            var status = _device.GetNextPacket(out var packet);
            if (isFirstPacket && status != GetPacketStatus.PacketRead)
            {
                // No new data and we are not reading any packet - return no data.
                return false;
            }
            if (status != GetPacketStatus.PacketRead)
            {
                break;
            }

            // Parse TCP packet.
            var rawPacket = packet.GetPacket();
            var parsedPacket = PacketDotNet.Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            _tcpPacket = parsedPacket.Extract<PacketDotNet.TcpPacket>();

            // Read and format payload.
            var payloadSpan = _tcpPacket.PayloadDataSegment.Bytes
                .AsSpan(_tcpPacket.PayloadDataSegment.Offset, _tcpPacket.PayloadDataSegment.Length);
            if (isFirstPacket)
            {
                if (payloadSpan.Length < 6)
                {
                    break;
                }
                actualPacketLength = BinaryPrimitives.ReadInt32BigEndian(payloadSpan[1..5]);
                isFirstPacket = false;
            }
            _buffer.Write(payloadSpan);
        }
        while (actualPacketLength >= _buffer.Size);

        return true;
    }

    /// <inheritdoc />
    public void Explain(IndentedStringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("pgsniffer");
    }
}
