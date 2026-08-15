using System.Reflection;
using Dapper;

namespace AjaiaDocs.Infrastructure.Data.Migrations;

public sealed class SchemaMigrationRunner(AjaiaDbConnectionFactory connections)
{
    private const string ResourcePrefix = "AjaiaDocs.Infrastructure.Data.Migrations.";

    public async Task MigrateAsync(CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var lockAcquired = false;

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "SELECT pg_advisory_lock(hashtext('ajaia-docs-migrations'));",
                cancellationToken: ct));
            lockAcquired = true;

            await connection.ExecuteAsync(new CommandDefinition(
                """
                CREATE TABLE IF NOT EXISTS schema_migrations (
                    version varchar(100) PRIMARY KEY,
                    applied_at timestamptz NOT NULL
                );
                """, cancellationToken: ct));

            foreach (var migration in GetMigrations())
            {
                var isApplied = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                    "SELECT EXISTS (SELECT 1 FROM schema_migrations WHERE version = @Version);",
                    new { migration.Version }, cancellationToken: ct));
                if (isApplied)
                {
                    continue;
                }

                await using var transaction = await connection.BeginTransactionAsync(ct);
                try
                {
                    await connection.ExecuteAsync(new CommandDefinition(migration.Sql,
                        transaction: transaction, cancellationToken: ct));
                    await connection.ExecuteAsync(new CommandDefinition(
                        "INSERT INTO schema_migrations (version, applied_at) VALUES (@Version, now());",
                        new { migration.Version }, transaction, cancellationToken: ct));
                    await transaction.CommitAsync(ct);
                }
                catch
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }
            }
        }
        finally
        {
            if (lockAcquired)
            {
                await connection.ExecuteAsync(
                    "SELECT pg_advisory_unlock(hashtext('ajaia-docs-migrations')); ");
            }
        }
    }

    private static IReadOnlyList<Migration> GetMigrations()
    {
        var assembly = typeof(SchemaMigrationRunner).Assembly;
        return assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal)
                && name.EndsWith(".sql", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .Select(name => ReadMigration(assembly, name))
            .ToArray();
    }

    private static Migration ReadMigration(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded migration '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();
        var version = resourceName[ResourcePrefix.Length..^".sql".Length];
        return new Migration(version, sql);
    }

    private sealed record Migration(string Version, string Sql);
}
