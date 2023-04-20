using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.Network.Inputs;

[Description("Network interfaces and relevant metadata.")]
[FunctionSignature("net_interface_addresses")]
internal sealed class InterfaceAddressesRowsInput : FetchInput<InterfaceAddressesRowsInput.InterfaceAddressDto>
{
    public class InterfaceAddressDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public NetworkInterfaceType Type { get; set; }

        public string Mac { get; set; } = string.Empty;

        public OperationalStatus Status { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Mask { get; set; } = string.Empty;

        public string Broadcast { get; set; } = string.Empty;

        public string Dns { get; set; } = string.Empty;

        public string DnsSuffix { get; set; } = string.Empty;

        public string Gateway { get; set; } = string.Empty;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<InterfaceAddressDto> builder)
    {
        // For reference: https://osquery.io/schema/5.7.0/#interface_addresses.
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id, "Interface identifier.")
            .AddProperty(p => p.Name, "Interface name.")
            .AddProperty(p => p.Type, "Interface type.")
            .AddProperty(p => p.Mac, "Interface physical address.")
            .AddProperty(p => p.Status, "Operational status.")
            .AddProperty(p => p.Address, "Interface address.")
            .AddProperty(p => p.Mask, "Interface address mask.")
            .AddProperty(p => p.Broadcast, "Broadcast address.")
            .AddProperty(p => p.Dns, "DNS servers.")
            .AddProperty(p => p.DnsSuffix, "DNS suffix.")
            .AddProperty(p => p.Gateway, "Gateway address.");
    }

    /// <inheritdoc />
    protected override IEnumerable<InterfaceAddressDto> GetData(Fetcher<InterfaceAddressDto> fetch)
    {
        var result = new List<InterfaceAddressDto>();
        foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
        {
            var ipProps = iface.GetIPProperties();
            foreach (var unicastIpAddressInformation in ipProps.UnicastAddresses)
            {
                result.Add(new InterfaceAddressDto
                {
                    Id = iface.Id,
                    Name = iface.Name,
                    Type = iface.NetworkInterfaceType,
                    Mac = string.Join(":", iface.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("x2"))),
                    Status = iface.OperationalStatus,
                    Address = unicastIpAddressInformation.Address.ToString(),
                    Mask = unicastIpAddressInformation.IPv4Mask.ToString(),
                    Broadcast = FormatIpAddresses(ipProps.MulticastAddresses),
                    Dns = FormatIpAddresses(ipProps.DnsAddresses),
                    DnsSuffix = ipProps.DnsSuffix,
                    Gateway = FormatIpAddresses(ipProps.GatewayAddresses),
                });
            }
        }
        return result;
    }

    private static string FormatIpAddresses(IEnumerable<IPAddressInformation> addressCollection)
    {
        return string.Join("; ", addressCollection.Select(a => a.Address.ToString()));
    }

    private static string FormatIpAddresses(IEnumerable<IPAddress> addressCollection)
    {
        return string.Join("; ", addressCollection.Select(a => a.ToString()));
    }

    private static string FormatIpAddresses(IEnumerable<GatewayIPAddressInformation> addressCollection)
    {
        return string.Join("; ", addressCollection.Select(a => a.Address.ToString()));
    }
}
