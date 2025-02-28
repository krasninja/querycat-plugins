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
    public static async ValueTask<VariantValue> SetTokenFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = thread.Stack.Pop().AsString;
        await thread.ConfigStorage.SetAsync(General.GitHubToken, new VariantValue(token), cancellationToken);
        return VariantValue.Null;
    }
}
