using System.ComponentModel;
using System.IO;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.FileSystem.Functions;

internal static class GetDir
{
    [Description("Get path of a file or directory.")]
    [FunctionSignature("fs_get_dir(path: string): string")]
    public static VariantValue GetDirFunction(FunctionCallInfo args)
    {
        var path = args.GetAt(0).AsString;
        return new VariantValue(Path.GetDirectoryName(path));
    }
}
