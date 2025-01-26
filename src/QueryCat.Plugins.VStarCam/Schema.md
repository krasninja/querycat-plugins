# Schema

- [vstar_camera_info](#vstar_camera_info)
- [vstar_cameras](#vstar_cameras)

## **vstar_camera_info**

```
QueryCat.Backend.FunctionsManager.DefaultFunctionsFactory+LazyAttributesFunction
```

VStar camera information.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` | yes | Camera identifier. |
| `ip` | `String` |  | IP address. |
| `ip_gateway` | `String` |  | IP gateway. |
| `port` | `Integer` |  | Port. |
| `ip_mask` | `String` |  | IP mask. |
| `name` | `String` |  | Camera name. |
| `firmware_version` | `String` |  | Firmware version. |
| `primary_dns` | `String` |  | Primary DNS. |
| `secondary_dns` | `String` |  | Secondary DNS. |
| `framerate` | `Integer` |  |  |
| `main_stream_width` | `Integer` |  | Main stream width. |
| `main_stream_height` | `Integer` |  | Main stream height. |
| `bitrate` | `Integer` |  | Bitrate. |
| `ir` | `Boolean` |  | Is IR enabled. |

## **vstar_cameras**

```
QueryCat.Backend.FunctionsManager.DefaultFunctionsFactory+LazyAttributesFunction
```

VStar cameras in local network.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` |  | Camera identifier. |
| `ip` | `String` |  | IP address. |
| `ip_gateway` | `String` |  | IP gateway. |
| `port` | `Integer` |  | Port. |
| `ip_mask` | `String` |  | IP mask. |
| `name` | `String` |  | Camera name. |
| `firmware_version` | `String` |  | Firmware version. |
| `primary_dns` | `String` |  | Primary DNS. |
| `secondary_dns` | `String` |  | Secondary DNS. |
