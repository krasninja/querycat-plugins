using System.ComponentModel;
using System.Net;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Network.Functions;

internal static class Hostname
{
    [SafeFunction]
    [Description("Get the host name of the local computer.")]
    [FunctionSignature("net_hostname(): string")]
    public static VariantValue HostnameFunction(FunctionCallInfo args)
    {
        return new VariantValue(Dns.GetHostName());
    }
}
