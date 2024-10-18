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
    public static VariantValue AwsSetTokenAuthFunction(IExecutionThread thread)
    {
        thread.ConfigStorage.Set(General.AwsAccessKey, thread.Stack[0]);
        thread.ConfigStorage.Set(General.AwsSecretKey, thread.Stack[1]);
        return VariantValue.Null;
    }
}
