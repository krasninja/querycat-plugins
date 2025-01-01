using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.System.Inputs;

internal sealed class EnvsRowsInput : EnumerableRowsInput<EnvsRowsInput.EnvDto>
{
    [SafeFunction]
    [Description("A key/value table of environment variables.")]
    [FunctionSignature("sys_envs(): object<IRowsInput>")]
    public static VariantValue EnvsRowsFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new EnvsRowsInput());
    }

    internal class EnvDto
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<EnvDto> builder)
    {
        // For reference: https://osquery.io/schema/5.7.0/#process_envs.
        builder
            .AddProperty("key", p => p.Key, "Environment variable name.")
            .AddProperty("value", p => p.Value, "Environment variable value.");
    }

    /// <inheritdoc />
    protected override IEnumerable<EnvDto> GetData(Fetcher<EnvDto> fetch)
    {
        var list = new List<EnvDto>();
        var enumerator = Environment.GetEnvironmentVariables().GetEnumerator();
        while (enumerator.MoveNext())
        {
            list.Add(new EnvDto
            {
                Key = (enumerator.Key ?? string.Empty).ToString() ?? string.Empty,
                Value = (enumerator.Value ?? string.Empty).ToString() ?? string.Empty,
            });
        }
        return list;
    }
}
