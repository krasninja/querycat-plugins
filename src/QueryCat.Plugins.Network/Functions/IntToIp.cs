using System.ComponentModel;
using System.Net;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Network.Functions;

internal static class IntToIp
{
    [SafeFunction]
    [Description("Convert integer to IP address.")]
    [FunctionSignature("net_int_to_ip(ip: integer): string")]
    public static VariantValue IntToIpFunction(IExecutionThread thread)
    {
        var ip = thread.Stack.Pop().AsInteger;
        var bytes = BitConverter.GetBytes((int)ip);
        return new VariantValue(new IPAddress(bytes).ToString());
    }
}
