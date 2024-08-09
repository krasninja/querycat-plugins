using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.Aws.Functions;
using QueryCat.Plugins.Aws.Inputs;

namespace QueryCat.Plugins.Aws;

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
        functionsManager.RegisterFunction(SetToken.AwsSetTokenAuthFunction);
        functionsManager.RegisterFromType(typeof(Ec2InstancesRowsInput));
    }
}
