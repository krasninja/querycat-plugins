# Schema

- [wevt_logs](#wevt_logs)

## **wevt_logs**

```
wevt_logs(path?: String): Object<IRowsInput>
```

The source contains all Windows event logs.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `datetime`| `Timestamp` |  |  | The data and time when the event was created. |
| `task`| `Integer` |  |  | Task identifier for a portion of an application or a component that publishes an event. |
| `level`| `Integer` |  |  | Level of the event. The level signifies the severity of the event. |
| `level_display_name`| `String` |  |  | Display name of the level. |
| `provider_name`| `String` |  |  | Event provider that published this event. |
| `provider_guid`| `Void` |  |  | Event provider identifier (GUID) that published this event. |
| `computer_name`| `String` |  |  | Computer name of the machine on which this event was logged. |
| `event_id`| `Integer` |  |  | Event identifier. |
| `record_id`| `Integer` |  |  | Record ID of the event. |
| `keywords`| `Integer` |  |  | The keyword mask of the even.t |
| `pid`| `Integer` |  |  | Process identifier that published this event. |
| `tid`| `Integer` |  |  | Thread identifier that published this event. |
| `activity_id`| `Void` |  |  | The globally unique identifier (GUID) for the activity in process for which the event is involved. |
| `related_activity_id`| `Void` |  |  | The globally unique identifier (GUID) for a related activity in a process for which an event is involved. |
| `log_name`| `String` |  |  | Name of the event log where this event is logged. |
| `user_id`| `Object` |  |  | The security descriptor of the user whose context is used to publish the event. |
| `path`| `String` |  |  | The provided query path. |
