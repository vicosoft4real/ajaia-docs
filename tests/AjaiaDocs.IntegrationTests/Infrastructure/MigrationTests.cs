using Dapper;
using Npgsql;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.CollectionName)]
public sealed class MigrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    [Fact]
    public async Task Migrations_are_idempotent_and_seed_exactly_three_users()
    {
        await fixture.Migrator.MigrateAsync(CancellationToken.None);
        await fixture.Migrator.MigrateAsync(CancellationToken.None);

        await using var connection = await fixture.OpenConnectionAsync();
        var users = (await connection.QueryAsync<SeededUser>(
            """
            SELECT id, email, display_name AS DisplayName, avatar_color AS AvatarColor
            FROM app_users
            WHERE is_seeded = true
            ORDER BY id
            """)).ToArray();

        Assert.Equal(3, users.Length);
        Assert.Equal(new SeededUser(DemoUsers.AminaId, "amina@example.test", "Amina Okafor", "#365CF5"), users[0]);
        Assert.Equal(new SeededUser(DemoUsers.ChidiId, "chidi@example.test", "Chidi Okeke", "#25A77A"), users[1]);
        Assert.Equal(new SeededUser(DemoUsers.TayoId, "tayo@example.test", "Tayo Bello", "#C77A15"), users[2]);
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM schema_migrations"));
    }

    [Fact]
    public async Task Schema_rejects_sharing_a_document_with_its_owner()
    {
        var documentId = Guid.NewGuid();
        await using var connection = await fixture.OpenConnectionAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO documents
                (id, owner_id, title, content_format, content, plain_text, version, created_at, updated_at)
            VALUES
                (@DocumentId, @OwnerId, 'Owner share guard', 'plainText', '', '', 1, now(), now())
            """, new { DocumentId = documentId, OwnerId = DemoUsers.AminaId });

        var exception = await Assert.ThrowsAsync<PostgresException>(() => connection.ExecuteAsync(
            """
            INSERT INTO document_shares (document_id, user_id, shared_by_user_id, created_at)
            VALUES (@DocumentId, @OwnerId, @OwnerId, now())
            """, new { DocumentId = documentId, OwnerId = DemoUsers.AminaId }));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => fixture.ResetAsync();

    private sealed record SeededUser(Guid Id, string Email, string DisplayName, string AvatarColor);
}
