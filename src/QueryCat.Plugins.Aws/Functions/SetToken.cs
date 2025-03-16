using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Aws.Functions;

internal static class SetToken
{
    [SafeFunction]
    [Description("Set AWS token authentication method.")]
    [FunctionSignature("aws_set_token(access_key: string, secret_key: string): void")]
    public static async ValueTask<VariantValue> AwsSetTokenAuthFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        await thread.ConfigStorage.SetAsync(General.AwsAccessKey, thread.Stack[0], cancellationToken);
        await thread.ConfigStorage.SetAsync(General.AwsSecretKey, thread.Stack[1], cancellationToken);
        return VariantValue.Null;
    }
}
