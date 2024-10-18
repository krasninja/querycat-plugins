using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Functions;

internal static class SetToken
{
    [SafeFunction]
    [Description("GitHub authentication.")]
    [FunctionSignature("github_set_token(token: string): void")]
    public static VariantValue SetTokenFunction(IExecutionThread thread)
    {
        var token = thread.Stack.Pop().AsString;
        thread.ConfigStorage.Set(General.GitHubToken, new VariantValue(token));
        return VariantValue.Null;
    }
}
