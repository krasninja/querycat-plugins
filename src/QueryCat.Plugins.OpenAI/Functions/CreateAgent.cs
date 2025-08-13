using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.OpenAI.Functions;

internal static class CreateAgent
{
    [SafeFunction]
    [Description("Create instance of OpenAI agent.")]
    [FunctionSignature("openai_agent(auth_key: string, model: string = 'gpt-4o-mini'): object<IAnswerAgent>")]
    // ReSharper disable once InconsistentNaming
    public static VariantValue OpenAIAgent(IExecutionThread thread)
    {
        var apiKey = thread.Stack[0].AsString;
        var model = thread.Stack[1].AsString;
        var agent = new OpenAiAnswerAgent(apiKey, model);
        return VariantValue.CreateFromObject(agent);
    }
}
