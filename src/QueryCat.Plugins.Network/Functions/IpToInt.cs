using System.ComponentModel;
using System.Net;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Network.Functions;

internal static class IpToInt
{
    [SafeFunction]
    [Description("Convert IP address to integer.")]
    [FunctionSignature("net_ip_to_int(ip: string): integer")]
    public static VariantValue IpToIntFunction(FunctionCallInfo args)
    {
        var ip = args.GetAt(0).AsString;
        if (IPAddress.TryParse(ip, out var address))
        {
            var bytes = address.GetAddressBytes();
            if (bytes.Length == 4)
            {
                return new VariantValue(BitConverter.ToUInt32(bytes, 0));
            }
            if (bytes.Length == 16 || bytes.Length == 8)
            {
                return new VariantValue(BitConverter.ToInt64(bytes, 0));
            }
        }
        return VariantValue.Null;
    }
}
