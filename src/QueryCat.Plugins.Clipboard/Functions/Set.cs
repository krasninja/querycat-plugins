using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using TextCopy;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Clipboard.Functions;

internal static class Set
{
    [Description("Clears the clipboard and then adds text data to it.")]
    [FunctionSignature("clipboard_set([text]: string): void")]
    public static VariantValue SetFunction(IExecutionThread thread)
    {
        var text = thread.Stack.Pop().AsString;
        ClipboardService.SetText(text);
        return VariantValue.Null;
    }
}
