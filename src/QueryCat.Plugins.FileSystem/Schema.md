# Schema

- [fs_dirs](#fs_dirs)
- [fs_files](#fs_files)

## **fs_dirs**

```
fs_dirs(): Object<IRowsInput>
```

Return information on directories.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `path`| `String` | yes |  | Full path of the directory. |
| `name`| `String` |  |  | Name of the directory. |
| `creation_time`| `Timestamp` |  |  | Date and time at which the directory has been created (in UTC). |
| `last_write_time`| `Timestamp` |  |  | Date and time at which the directory has been last modified (in UTC). |
| `last_access_time`| `Timestamp` |  |  | Date and time at which the directory has been last accessed (in UTC). |
| `attributes`| `String` |  |  | directory attributes. |
| `unix_file_mode`| `String` |  |  | UNIX directory permissions. |

## **fs_files**

```
fs_files(): Object<IRowsInput>
```

Return information on files.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `path`| `String` | yes |  | Full path of the file. |
| `name`| `String` |  |  | Name of the file. |
| `size`| `Integer` |  |  | Size of the file, in bytes. |
| `is_read_only`| `Boolean` |  |  | Is file read only. |
| `creation_time`| `Timestamp` |  |  | Date and time at which the file has been created (in UTC). |
| `last_write_time`| `Timestamp` |  |  | Date and time at which the file has been last modified (in UTC). |
| `last_access_time`| `Timestamp` |  |  | Date and time at which the file has been last accessed (in UTC). |
| `attributes`| `String` |  |  | File attributes. |
| `unix_file_mode`| `String` |  |  | UNIX file permissions. |
