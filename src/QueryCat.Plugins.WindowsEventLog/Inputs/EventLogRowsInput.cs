using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.WindowsEventLog.Inputs;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "For non-Windows platforms returns nothing.")]
internal sealed class EventLogRowsInput : EnumerableRowsInput<EventRecord>
{
    private readonly string _path;

    [SafeFunction]
    [Description("The source contains all Windows event logs.")]
    [FunctionSignature("wevt_logs(path?: string): object<IRowsInput>")]
    public static VariantValue WindowsEventLogsFunction(IExecutionThread thread)
    {
        var path = thread.Stack.Pop();
        return VariantValue.CreateFromObject(new EventLogRowsInput(path.AsString));
    }

    public EventLogRowsInput(string path)
    {
        _path = path;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<EventRecord> builder)
    {
        // For reference: https://osquery.io/schema/5.16.0/#windows_eventlog.
        builder
            .AddProperty("datetime", p => p.TimeCreated, "The data and time when the event was created.")
            .AddProperty("task", p => p.Task, "Task identifier for a portion of an application or a component that publishes an event.")
            .AddProperty("level", p => p.Level, "Level of the event. The level signifies the severity of the event.")
            .AddProperty("level_display_name", p => p.LevelDisplayName, "Display name of the level.")
            .AddProperty("provider_name", p => p.ProviderName, "Event provider that published this event.")
            .AddProperty("provider_guid", p => p.ProviderId, "Event provider identifier (GUID) that published this event.")
            .AddProperty("computer_name", p => p.MachineName, "Computer name of the machine on which this event was logged.")
            .AddProperty("event_id", p => p.Id, "Event identifier.")
            .AddProperty("record_id", p => p.RecordId, "Record ID of the event.")
            .AddProperty("keywords", p => p.Keywords, "The keyword mask of the even.t")
            .AddProperty("pid", p => p.ProcessId, "Process identifier that published this event.")
            .AddProperty("tid", p => p.ThreadId, "Thread identifier that published this event.")
            .AddProperty("activity_id", p => p.ActivityId, "The globally unique identifier (GUID) for the activity in process for which the event is involved.")
            .AddProperty("related_activity_id", p => p.RelatedActivityId, "The globally unique identifier (GUID) for a related activity in a process for which an event is involved.")
            .AddProperty("log_name", p => p.LogName, "Name of the event log where this event is logged.")
            .AddProperty("user_id", p => p.UserId, "The security descriptor of the user whose context is used to publish the event.")
            .AddProperty("path", _ => _path, "The provided query path.");
    }

    /// <inheritdoc />
    protected override IEnumerable<EventRecord> GetData(Fetcher<EventRecord> fetcher)
    {
        if (!OperatingSystem.IsWindows())
        {
            yield break;
        }

        using var reader = new EventLogReader(_path);
        for (var ev = reader.ReadEvent(); ev != null; ev = reader.ReadEvent())
        {
            yield return ev;
        }
    }
}
