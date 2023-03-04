using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.System.Inputs;

[Description("A key/value table of command line arguments.")]
[FunctionSignature("sys_args")]
internal sealed class ArgsRowsInput : ClassEnumerableInput<ArgsRowsInput.ArgDto>
{
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
    protected override IEnumerable<ArgDto> GetData(ClassEnumerableInputFetch<ArgDto> fetch)
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
