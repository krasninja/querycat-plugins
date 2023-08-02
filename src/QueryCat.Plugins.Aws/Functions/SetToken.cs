using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Aws.Functions;

internal static class SetToken
{
    [Description("Set AWS token authentication method.")]
    [FunctionSignature("aws_set_token(access_key: string, secret_key: string): void")]
    public static VariantValue AwsSetTokenAuthFunction(FunctionCallInfo args)
    {
        args.ExecutionThread.ConfigStorage.Set(General.AwsAccessKey, args.GetAt(0));
        args.ExecutionThread.ConfigStorage.Set(General.AwsSecretKey, args.GetAt(1));
        return VariantValue.Null;
    }
}
