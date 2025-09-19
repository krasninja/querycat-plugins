# File System Plugin

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

## Examples

**Get files by pattern**

```sql
select path from fs_files() where path like '/home/ivan/work/project/crm/CRM.Management.Database/%Tables%.sql'
```

**Get directories by pattern**

```sql
select path from fs_dirs() where path like '/home/ivan/work/project/crm/CRM.Management.Database/%Tables%'
```
