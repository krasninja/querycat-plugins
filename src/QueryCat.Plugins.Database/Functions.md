# Functions

| Name and Description |
| --- |
| `duckdb_table(cs: String, table: String): Object<IRowsInput>`<br /><br /> Returns data from DuckDB database table. |
| `duckdb_table_out(cs: String, table: String, keys: String := '', skip_updates: Boolean := False): Object<IRowsOutput>`<br /><br /> Writes data to DuckDB database table. |
| `pg_table(cs: String, table: String): Object<IRowsInput>`<br /><br /> Returns data from Postgres database table. |
| `pg_table_out(cs: String, table: String, keys: String := '', skip_updates: Boolean := False): Object<IRowsOutput>`<br /><br /> Writes data to Postgres database table. |
| `sqlite_table(cs: String, table: String): Object<IRowsInput>`<br /><br /> Returns data from SQLite database table. |
| `sqlite_table_out(cs: String, table: String, keys: String := '', skip_updates: Boolean := False): Object<IRowsOutput>`<br /><br /> Writes data to SQLite database table. |
