using System.ComponentModel;
using TextCopy;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Clipboard.Functions;

internal static class Set
{
    [Description("Clears the clipboard and then adds text data to it.")]
    [FunctionSignature("clipboard_set(text: string): void")]
    public static VariantValue SetFunction(FunctionCallInfo args)
    {
        var text = args.GetAt(0).AsString;
        ClipboardService.SetText(text);
        return VariantValue.Null;
    }
}
