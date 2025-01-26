# Schema

- [sys_args](#sys_args)
- [sys_envs](#sys_envs)
- [sys_processes](#sys_processes)

## **sys_args**

```
sys_args(): Object<IRowsInput>
```

A key/value table of command line arguments.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `value`| `String` |  |  | Command line argument. |

## **sys_envs**

```
sys_envs(): Object<IRowsInput>
```

A key/value table of environment variables.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `key`| `String` |  |  | Environment variable name. |
| `value`| `String` |  |  | Environment variable value. |

## **sys_processes**

```
sys_processes(): Object<IRowsInput>
```

All running processes on the host system.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `pid`| `Integer` |  |  | Process id. |
| `name`| `String` |  |  | Process path. |
| `command`| `String` |  |  | Process command line. |
| `base_priority`| `Integer` |  |  | Base process priority. |
| `start_time`| `Timestamp` |  |  | Process start time. |
| `working_set`| `Integer` |  |  | Amount of physical memory. |
| `private_set`| `Integer` |  |  | Amount of private memory. |
| `virtual_set`| `Integer` |  |  | Amount of virtual memory. |
