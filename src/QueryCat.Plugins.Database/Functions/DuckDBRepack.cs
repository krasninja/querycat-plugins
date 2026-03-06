using System.ComponentModel;
using System.Text;
using DuckDB.NET.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Database.Functions;

internal static class DuckDBRepack
{
    [Description("Copies DuckDB database to a new file, which can help to reduce the file size after many updates and deletes.")]
    [FunctionSignature("duckdb_repack(path: string): void")]
    public static async ValueTask<VariantValue> DuckDBRepackFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var path = thread.Stack.Pop().AsString;

        await using var connection = new DuckDBConnection(DuckDBConnectionStringBuilder.InMemoryConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var checkpointCommand = connection.CreateCommand();
        var tempdb = string.Concat(
            Path.ChangeExtension(path, string.Empty),
            Guid.NewGuid().ToString("N"),
            ".db"
        );
        var sb = new StringBuilder()
            .AppendLine($"ATTACH '{Quote(path)}' AS sdb;")
            .AppendLine($"ATTACH '{Quote(tempdb)}' AS ddb;")
            .AppendLine("COPY FROM DATABASE sdb TO ddb;")
            .AppendLine("DETACH sdb;")
            .AppendLine("DETACH ddb;");
#pragma warning disable CA2100
        checkpointCommand.CommandText = sb.ToString();
#pragma warning restore CA2100

        try
        {
            await checkpointCommand.ExecuteNonQueryAsync(cancellationToken);
            await connection.CloseAsync();
            File.Move(tempdb, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempdb))
            {
                File.Delete(tempdb);
            }
        }

        return VariantValue.Null;
    }

    private static string Quote(string id) => id.Replace("\'", "''");
}
