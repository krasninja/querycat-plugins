# Database Plugin for QueryCat

- [Functions](Functions.md)
- [Changelog](CHANGELOG.md)

## Connections Strings

There are examples of input and output usage with connection strings:

### Postgres

```
delete from pg_table('Server=host;Database=db;Uid=postgres;Pwd=password;', 'table') where column1 > 2;
```

### SQLite

```
select * from sqlite_table('Data Source=/home/ivan/test.db');
```
