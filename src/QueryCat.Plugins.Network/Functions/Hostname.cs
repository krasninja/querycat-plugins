using System.ComponentModel;
using System.Net;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Network.Functions;

internal static class Hostname
{
    [Description("Get the host name of the local computer.")]
    [FunctionSignature("net_hostname(): string")]
    public static VariantValue HostnameFunction(FunctionCallInfo args)
    {
        return new VariantValue(Dns.GetHostName());
    }
}
