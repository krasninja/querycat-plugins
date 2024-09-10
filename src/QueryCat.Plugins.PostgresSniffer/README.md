# QueryCat Plugin for Postgres Sniffer

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

## Prerequisites

- Windows
  - Install [NPCAP](https://npcap.com/#download).
- Linux
  - Install [libpcap](https://www.tcpdump.org/manpages/pcap.3pcap.html) for your distribution.

## Example

**Run on the first network interface:**

```shell
qcat query "select * from pgsniff_start()" --follow --page-size=-1
```
