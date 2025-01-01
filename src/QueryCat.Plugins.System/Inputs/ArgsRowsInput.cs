using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.System.Inputs;

internal sealed class ArgsRowsInput : EnumerableRowsInput<ArgsRowsInput.ArgDto>
{
    [SafeFunction]
    [Description("A key/value table of command line arguments.")]
    [FunctionSignature("sys_args(): object<IRowsInput>")]
    public static VariantValue ArgsRowsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new ArgsRowsInput());
    }

    internal class ArgDto
    {
        public string Value { get; set; } = string.Empty;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<ArgDto> builder)
    {
        builder
            .AddProperty("value", p => p.Value, "Command line argument.");
    }

    /// <inheritdoc />
    protected override IEnumerable<ArgDto> GetData(Fetcher<ArgDto> fetch)
    {
        var list = new List<ArgDto>();
        var args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            list.Add(new ArgDto
            {
                Value = arg,
            });
        }
        return list;
    }
}
