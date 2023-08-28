using System.ComponentModel;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Functions;

internal static class SetToken
{
    [Description("GitHub authentication.")]
    [FunctionSignature("github_set_token(token: string): void")]
    public static VariantValue SetTokenFunction(FunctionCallInfo args)
    {
        var token = args.GetAt(0).AsString;
        args.ExecutionThread.ConfigStorage.Set(General.GitHubToken, new VariantValue(token));
        return VariantValue.Null;
    }
}
