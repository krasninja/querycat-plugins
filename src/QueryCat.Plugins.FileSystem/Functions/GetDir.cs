using System.ComponentModel;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.FileSystem.Functions;

internal static class GetDir
{
    [SafeFunction]
    [Description("Get path of a file or directory.")]
    [FunctionSignature("fs_get_dir(path: string): string")]
    public static VariantValue GetDirFunction(FunctionCallInfo args)
    {
        var path = args.GetAt(0).AsString;
        return new VariantValue(Path.GetDirectoryName(path));
    }
}
