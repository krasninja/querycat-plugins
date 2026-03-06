using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetToken
{
    [SafeFunction]
    [Description("Set JIRA token authentication method.")]
    [FunctionSignature("jira_set_token(username: string, token: string): void")]
    public static async ValueTask<VariantValue> JiraSetTokenAuthFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var username = thread.Stack[0];
        var token = thread.Stack[1];
        await thread.ConfigStorage.SetAsync(General.JiraUsername, username, cancellationToken);
        await thread.ConfigStorage.SetAsync(General.JiraToken, token, cancellationToken);
        return VariantValue.Null;
    }
}
