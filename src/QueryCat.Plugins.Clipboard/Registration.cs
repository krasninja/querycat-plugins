using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Clipboard.Functions;

namespace QueryCat.Plugins.Clipboard;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
internal static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(Get.GetFunction);
        functionsManager.RegisterFunction(Set.SetFunction);
    }
}
