using System.ComponentModel;
using System.Diagnostics;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.System.System;

internal class ProcessesRowsInput : ClassEnumerableInput<ProcessesRowsInput.ProcessDto>
{
    [Description("All running processes on the host system.")]
    [FunctionSignature("sys_ps(): object<IRowsInput>")]
    public static VariantValue SystemProcesses(FunctionCallInfo args)
    {
        return VariantValue.CreateFromObject(new ProcessesRowsInput());
    }

    public class ProcessDto
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
        builder.AddProperty("pid", p => p.Pid, "Process id.");
        builder.AddProperty("name", p => p.FileName, "Process path.");
        builder.AddProperty("command", p => p.Command, "Process command line.");
        builder.AddProperty("base_priority", p => p.BasePriority, "Base process priority.");
        builder.AddProperty("start_time", p => p.StartTime, "Process start time.");
        builder.AddProperty("working_set", p => p.WorkingSet, "Amount of physical memory.");
        builder.AddProperty("private_set", p => p.PrivateSet, "Amount of private memory.");
        builder.AddProperty("virtual_set", p => p.VirtualSet, "Amount of virtual memory.");
    }

    /// <inheritdoc />
    protected override IEnumerable<ProcessDto> GetData(ClassEnumerableInputFetch<ProcessDto> fetch)
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
