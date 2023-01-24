using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Aws;

/// <summary>
/// AWS functions.
/// </summary>
internal static class Functions
{
    public const string GitHubToken = "aws-access-key";

    [Description("AWS authentication.")]
    [FunctionSignature("aws_set(token: string): void")]
    public static VariantValue GitHubSetTokenFunction(FunctionCallInfo args)
    {
        var token = args.GetAt(0).AsString;
        args.ExecutionThread.ConfigStorage.Set(GitHubToken, new VariantValue(token));
        return VariantValue.Null;
    }
}
