using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using TextCopy;

namespace QueryCat.Plugins.Clipboard.Functions;

internal static class Get
{
    [SafeFunction]
    [Description("Retrieves text data from the clipboard.")]
    [FunctionSignature("clipboard_get(): string")]
    public static VariantValue GetFunction(IExecutionThread thread)
    {
        var text = ClipboardService.GetText();
        return new VariantValue(text);
    }
}
