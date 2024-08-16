using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Network.Functions;
using QueryCat.Plugins.Network.Inputs;

namespace QueryCat.Plugins.Network;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
public static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(Hostname.HostnameFunction);
        functionsManager.RegisterFunction(IntToIp.IntToIpFunction);
        functionsManager.RegisterFunction(IpToInt.IpToIntFunction);
        functionsManager.RegisterFunction(InterfaceAddressesRowsInput.InterfaceAddressesFunction);
        functionsManager.RegisterFunction(InterfaceDetailsRowsInput.InterfaceDetailsFunction);
    }
}
