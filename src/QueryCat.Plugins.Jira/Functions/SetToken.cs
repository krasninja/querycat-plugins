using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetToken
{
    [SafeFunction]
    [Description("Set JIRA token authentication method.")]
    [FunctionSignature("jira_set_token(token: string): void")]
    public static VariantValue JiraSetTokenAuthFunction(IExecutionThread thread)
    {
        thread.ConfigStorage.Set(General.JiraToken, thread.Stack.Pop());
        return VariantValue.Null;
    }
}
