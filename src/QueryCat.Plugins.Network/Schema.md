# Schema

- [net_interface_addresses](#net_interface_addresses)
- [net_interface_details](#net_interface_details)

## **net_interface_addresses**

```
net_interface_addresses(): Object<IRowsInput>
```

Network interfaces and relevant metadata.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` |  | Interface identifier. |
| `name` | `String` |  | Interface name. |
| `type` | `String` |  | Interface type. |
| `mac` | `String` |  | Interface physical address. |
| `status` | `String` |  | Operational status. |
| `address` | `String` |  | Interface address. |
| `mask` | `String` |  | Interface address mask. |
| `broadcast` | `String` |  | Broadcast address. |
| `dns` | `String` |  | DNS servers. |
| `dns_suffix` | `String` |  | DNS suffix. |
| `gateway` | `String` |  | Gateway address. |

## **net_interface_details**

```
net_interface_details(): Object<IRowsInput>
```

Network interfaces and relevant detailed information.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` |  | Interface identifier. |
| `name` | `String` |  | Interface name. |
| `description` | `String` |  | Interface description. |
| `type` | `String` |  | Interface type. |
| `mac` | `String` |  | Interface physical address. |
| `status` | `String` |  | Operational status. |
| `multicast` | `Boolean` |  | Supports multicast. |
| `speed` | `Integer` |  | Interface link speed. |
| `ipackets` | `Integer` |  | Packets received. |
| `opackets` | `Integer` |  | Packets sent. |
| `ibytes` | `Integer` |  | Bytes received. |
| `obytes` | `Integer` |  | Bytes sent. |
| `ierrors` | `Integer` |  | Number of incoming packets with errors. |
| `oerrors` | `Integer` |  | Number of outgoing packets with errors. |
| `ip` | `String` |  |  |
