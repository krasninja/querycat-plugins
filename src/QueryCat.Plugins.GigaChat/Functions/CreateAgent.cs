using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.GigaChat.Functions;

internal static class CreateAgent
{
    [SafeFunction]
    [Description("Create instance of GigaChat AI agent.")]
    [FunctionSignature("gigachat_agent(auth_key: string): object<IAnswerAgent>")]
    public static VariantValue GigaChatAgent(IExecutionThread thread)
    {
        var apiKey = thread.Stack.Pop();
        var agent = new GigaChatAnswerAgent(apiKey);
        return VariantValue.CreateFromObject(agent);
    }
}
