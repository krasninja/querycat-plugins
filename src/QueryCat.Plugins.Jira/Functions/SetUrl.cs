using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Jira.Functions;

internal static class SetUrl
{
    [Description("Set JIRA instance URL.")]
    [FunctionSignature("jira_set_url(url: string): void")]
    public static VariantValue JiraSetUrlFunction(FunctionCallInfo args)
    {
        args.ExecutionThread.ConfigStorage.Set(General.JiraUrl, args.GetAt(0));
        return VariantValue.Null;
    }
}