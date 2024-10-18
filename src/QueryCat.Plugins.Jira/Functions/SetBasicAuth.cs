using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetBasicAuth
{
    [SafeFunction]
    [Description("Set JIRA basic authentication method.")]
    [FunctionSignature("jira_set_basic_auth(username: string, password: string): void")]
    public static VariantValue JiraSetBasicAuthFunction(IExecutionThread thread)
    {
        thread.ConfigStorage.Set(General.JiraUsername, thread.Stack[0]);
        thread.ConfigStorage.Set(General.JiraPassword, thread.Stack[1]);
        return VariantValue.Null;
    }
}
