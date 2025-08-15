using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.DeepSeek.Functions;

internal static class CreateAgent
{
    [SafeFunction]
    [Description("Create instance of DeepSeek AI agent.")]
    [FunctionSignature("deepseek_agent(api_key: string, model: string = 'deepseek-chat'): object<IAnswerAgent>")]
    public static VariantValue DeepSeekAgent(IExecutionThread thread)
    {
        var apiKey = thread.Stack[0].AsString;
        var model = thread.Stack[1].AsString;
        var agent = new DeepSeekAnswerAgent(apiKey, model);
        return VariantValue.CreateFromObject(agent);
    }
}
