using AjaiaDocs.Infrastructure.Data;
using AjaiaDocs.Infrastructure.Data.Migrations;
using AjaiaDocs.Infrastructure.Data.Repositories;
using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string CollectionName = "PostgreSQL integration tests";
}

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("ajaia_docs_tests")
        .WithUsername("ajaia_docs")
        .WithPassword("ajaia_docs_tests")
        .Build();

    private AjaiaDbConnectionFactory? _connections;

    public SchemaMigrationRunner Migrator { get; private set; } = null!;

    public DocumentRepository Documents { get; private set; } = null!;

    public UserRepository Users { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connections = new AjaiaDbConnectionFactory(_container.GetConnectionString());
        Migrator = new SchemaMigrationRunner(_connections);
        Documents = new DocumentRepository(_connections);
        Users = new UserRepository(_connections);
        await Migrator.MigrateAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        if (_connections is not null)
        {
            await _connections.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    public Task<NpgsqlConnection> OpenConnectionAsync() =>
        _connections!.OpenConnectionAsync(CancellationToken.None);

    public async Task ResetAsync()
    {
        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(
            "TRUNCATE TABLE document_shares, documents RESTART IDENTITY;");
    }
}
