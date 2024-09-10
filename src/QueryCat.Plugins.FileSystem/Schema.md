# Schema

- [fs_files](#fs_files)

## **fs_files**

```
fs_files(): Object<IRowsInput>
```

Return information on files.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `path` | `String` | yes | Full path of the file. |
| `name` | `String` |  | Name of the file. |
| `size` | `Integer` |  | Size of the file, in bytes. |
| `is_read_only` | `Boolean` |  | Is file read only. |
| `creation_time` | `Timestamp` |  | Date and time at which the file has been created (in UTC). |
| `last_write_time` | `Timestamp` |  | Date and time at which the file has been last modified (in UTC). |
| `last_access_time` | `Timestamp` |  | Date and time at which the file has been last accessed (in UTC). |
| `attributes` | `String` |  | File attributes. |
| `unix_file_mode` | `String` |  | UNIX file permissions. |
