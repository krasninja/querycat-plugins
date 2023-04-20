using System.ComponentModel;
using System.Net.NetworkInformation;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.Network.Inputs;

[Description("Network interfaces and relevant detailed information.")]
[FunctionSignature("net_interface_details")]
internal sealed class InterfaceDetailsRowsInput : FetchInput<InterfaceDetailsRowsInput.InterfaceAddressDto>
{
    public class InterfaceAddressDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public NetworkInterfaceType Type { get; set; }

        public string Mac { get; set; } = string.Empty;

        public OperationalStatus Status { get; set; }

        public bool HasMulticast { get; set; }

        public long Speed { get; set; }

        public long PacketsReceived { get; set; }

        public long PacketsSent { get; set; }

        public long BytesReceived { get; set; }

        public long BytesSent { get; set; }

        public long IncomingPacketsErrors { get; set; }

        public long OutgoingPacketsErrors { get; set; }

        public string IpAddresses { get; set; } = string.Empty;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<InterfaceAddressDto> builder)
    {
        // For reference: https://osquery.io/schema/5.7.0/#interface_addresses.
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id, "Interface identifier.")
            .AddProperty(p => p.Name, "Interface name.")
            .AddProperty(p => p.Description, "Interface description.")
            .AddProperty(p => p.Type, "Interface type.")
            .AddProperty(p => p.Mac, "Interface physical address.")
            .AddProperty(p => p.Status, "Operational status.")
            .AddProperty("multicast", p => p.HasMulticast, "Supports multicast.")
            .AddProperty(p => p.Speed, "Interface link speed.")
            .AddProperty("ipackets", p => p.PacketsReceived, "Packets received.")
            .AddProperty("opackets", p => p.PacketsSent, "Packets sent.")
            .AddProperty("ibytes", p => p.BytesReceived, "Bytes received.")
            .AddProperty("obytes", p => p.BytesSent, "Bytes sent.")
            .AddProperty("ierrors", p => p.IncomingPacketsErrors, "Number of incoming packets with errors.")
            .AddProperty("oerrors", p => p.OutgoingPacketsErrors, "Number of outgoing packets with errors.")
            .AddProperty("ip", p => p.IpAddresses, "");
    }

    /// <inheritdoc />
    protected override IEnumerable<InterfaceAddressDto> GetData(Fetcher<InterfaceAddressDto> fetch)
    {
        var result = new List<InterfaceAddressDto>();
        foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
        {
            var props = iface.GetIPProperties();
            var stat = iface.GetIPStatistics();
            result.Add(new InterfaceAddressDto
            {
                Id = iface.Id,
                Name = iface.Name,
                Description = iface.Description,
                Type = iface.NetworkInterfaceType,
                Mac = string.Join(":", iface.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("x2"))),
                Status = iface.OperationalStatus,
                HasMulticast = iface.SupportsMulticast,
                Speed = iface.Speed,
                PacketsReceived = stat.UnicastPacketsReceived,
                PacketsSent = stat.UnicastPacketsSent,
                BytesReceived = stat.BytesReceived,
                BytesSent = stat.BytesSent,
                IncomingPacketsErrors = stat.IncomingPacketsWithErrors,
                OutgoingPacketsErrors = stat.OutgoingPacketsWithErrors,
                IpAddresses = FormatIpAddresses(props.UnicastAddresses),
            });
        }
        return result;
    }

    private static string FormatIpAddresses(IEnumerable<IPAddressInformation> addressCollection)
    {
        return string.Join("; ", addressCollection.Select(a => a.Address.ToString()));
    }
}
