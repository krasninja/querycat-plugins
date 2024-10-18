using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetUrl
{
    [SafeFunction]
    [Description("Set JIRA instance URL.")]
    [FunctionSignature("jira_set_url(url: string): void")]
    public static VariantValue JiraSetUrlFunction(IExecutionThread thread)
    {
        thread.ConfigStorage.Set(General.JiraUrl, thread.Stack.Pop());
        return VariantValue.Null;
    }
}
