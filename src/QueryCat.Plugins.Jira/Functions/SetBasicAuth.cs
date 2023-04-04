using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetBasicAuth
{
    [Description("Set JIRA basic authentication method.")]
    [FunctionSignature("jira_set_basic_auth(username: string, password: string): void")]
    public static VariantValue JiraSetBasicAuthFunction(FunctionCallInfo args)
    {
        args.ExecutionThread.ConfigStorage.Set(General.JiraUsername, args.GetAt(0));
        args.ExecutionThread.ConfigStorage.Set(General.JiraPassword, args.GetAt(1));
        return VariantValue.Null;
    }
}
