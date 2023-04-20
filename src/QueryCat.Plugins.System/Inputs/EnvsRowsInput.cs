using System.ComponentModel;
using System.Diagnostics;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.System.Inputs;

[Description("A key/value table of environment variables.")]
[FunctionSignature("sys_envs")]
internal sealed class EnvsRowsInput : FetchInput<EnvsRowsInput.EnvDto>
{
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
