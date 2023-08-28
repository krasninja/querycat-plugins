using System.ComponentModel;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetToken
{
    [Description("Set JIRA token authentication method.")]
    [FunctionSignature("jira_set_token(token: string): void")]
    public static VariantValue JiraSetTokenAuthFunction(FunctionCallInfo args)
    {
        args.ExecutionThread.ConfigStorage.Set(General.JiraToken, args.GetAt(0));
        return VariantValue.Null;
    }
}
