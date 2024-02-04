using System.ComponentModel;
using System.Diagnostics;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.System.Inputs;

[Description("All running processes on the host system.")]
[FunctionSignature("sys_processes")]
internal sealed class ProcessesRowsInput : FetchRowsInput<ProcessesRowsInput.ProcessDto>
{
    [SafeFunction]
    [Description("All running processes on the host system.")]
    [FunctionSignature("sys_processes(): object<IRowsInput>")]
    public static VariantValue ProcessesFunction(FunctionCallInfo args)
    {
        return VariantValue.CreateFromObject(new ProcessesRowsInput());
    }

    internal class ProcessDto
    {
        public int Pid { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Command { get; set; } = string.Empty;

        public int BasePriority { get; set; }

        public DateTime StartTime { get; set; }

        public long WorkingSet { get; set; }

        public long PrivateSet { get; set; }

        public long VirtualSet { get; set; }
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<ProcessDto> builder)
    {
        // For reference: https://osquery.io/schema/5.5.1/#processes.
        builder
            .AddProperty("pid", p => p.Pid, "Process id.")
            .AddProperty("name", p => p.FileName, "Process path.")
            .AddProperty("command", p => p.Command, "Process command line.")
            .AddProperty("base_priority", p => p.BasePriority, "Base process priority.")
            .AddProperty("start_time", p => p.StartTime, "Process start time.")
            .AddProperty("working_set", p => p.WorkingSet, "Amount of physical memory.")
            .AddProperty("private_set", p => p.PrivateSet, "Amount of private memory.")
            .AddProperty("virtual_set", p => p.VirtualSet, "Amount of virtual memory.");
    }

    /// <inheritdoc />
    protected override IEnumerable<ProcessDto> GetData(Fetcher<ProcessDto> fetch)
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            var dto = new ProcessDto
            {
                Pid = process.Id,
                FileName = process.MainModule?.FileName ?? string.Empty,
                Command = process.ProcessName,
                BasePriority = process.BasePriority,
                StartTime = process.StartTime,
                WorkingSet = process.WorkingSet64,
                PrivateSet = process.PrivateMemorySize64,
                VirtualSet = process.VirtualMemorySize64
            };
            yield return dto;
        }
    }
}
