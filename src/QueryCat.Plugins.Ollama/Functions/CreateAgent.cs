using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Ollama.Functions;

internal static class CreateAgent
{
    [SafeFunction]
    [Description("Create instance of Ollama agent.")]
    [FunctionSignature("ollama_agent(model: string, url: string = 'http://localhost:11434'): object<IAnswerAgent>")]
    // ReSharper disable once InconsistentNaming
    public static VariantValue OllamaAgent(IExecutionThread thread)
    {
        var model = thread.Stack[0].AsString;
        var url = thread.Stack[1].AsString;
        var agent = new OllamaAnswerAgent(model, url);
        return VariantValue.CreateFromObject(agent);
    }
}
