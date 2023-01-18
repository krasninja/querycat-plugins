using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Github;

/// <summary>
/// GitHub functions.
/// </summary>
internal static class Functions
{
    public const string GitHubToken = "github-access-token";

    [Description("GitHub authentication.")]
    [FunctionSignature("github_set_token(token: string): void")]
    public static VariantValue GitHubSetTokenFunction(FunctionCallInfo args)
    {
        var token = args.GetAt(0).AsString;
        args.ExecutionThread.ConfigStorage.Set(GitHubToken, new VariantValue(token));
        return VariantValue.Null;
    }
}
