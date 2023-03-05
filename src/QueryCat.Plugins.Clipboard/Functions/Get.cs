using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;
using TextCopy;

namespace QueryCat.Plugins.Clipboard.Functions;

internal static class Get
{
    [Description("Retrieves text data from the clipboard.")]
    [FunctionSignature("clipboard_get(): string")]
    public static VariantValue GetFunction(FunctionCallInfo args)
    {
        var text = ClipboardService.GetText();
        return new VariantValue(text);
    }
}
