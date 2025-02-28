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
    public static async ValueTask<VariantValue> JiraSetUrlFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        await thread.ConfigStorage.SetAsync(General.JiraUrl, thread.Stack.Pop(), cancellationToken);
        return VariantValue.Null;
    }
}
