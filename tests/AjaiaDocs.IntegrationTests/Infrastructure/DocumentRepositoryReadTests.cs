using AjaiaDocs.Application.Common;
using AjaiaDocs.Core.Documents;
using Dapper;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.CollectionName)]
public sealed class DocumentRepositoryReadTests(PostgresFixture fixture) : IAsyncLifetime
{
    [Fact]
    public async Task Create_and_get_project_owner_identity_and_capabilities()
    {
        var document = CreateDocument(DemoUsers.AminaId, "Product brief", DateTimeOffset.UtcNow);

        var created = await fixture.Documents.CreateAsync(document, CancellationToken.None);
        var fetched = await fixture.Documents.GetAsync(DemoUsers.AminaId, document.Id, CancellationToken.None);

        Assert.True(created.IsSuccess);
        Assert.True(fetched.IsSuccess);
        Assert.Equal(created.Value, fetched.Value);
        Assert.Equal("lexical", fetched.Value.ContentFormat);
        Assert.Equal("Amina Okafor", fetched.Value.Owner.DisplayName);
        Assert.Equal("#365CF5", fetched.Value.Owner.AvatarColor);
        Assert.True(fetched.Value.IsOwner);
        Assert.True(fetched.Value.CanEdit);
        Assert.True(fetched.Value.CanRename);
        Assert.True(fetched.Value.CanShare);
        Assert.True(fetched.Value.CanDelete);
    }

    [Fact]
    public async Task Shared_scope_returns_only_documents_shared_with_actor()
    {
        var owned = CreateDocument(DemoUsers.ChidiId, "Owned", DateTimeOffset.UtcNow.AddMinutes(-2));
        var shared = CreateDocument(DemoUsers.AminaId, "Shared", DateTimeOffset.UtcNow.AddMinutes(-1));
        await fixture.Documents.CreateAsync(owned, CancellationToken.None);
        await fixture.Documents.CreateAsync(shared, CancellationToken.None);
        await ShareAsync(shared.Id, DemoUsers.ChidiId, DemoUsers.AminaId);

        var rows = await fixture.Documents.ListAsync(DemoUsers.ChidiId,
            DocumentScope.Shared, CancellationToken.None);

        Assert.True(rows.IsSuccess);
        var row = Assert.Single(rows.Value);
        Assert.Equal(shared.Id, row.Id);
        Assert.False(row.IsOwner);
    }

    [Fact]
    public async Task Owned_scope_returns_only_documents_owned_by_actor()
    {
        var owned = CreateDocument(DemoUsers.AminaId, "Owned", DateTimeOffset.UtcNow);
        var shared = CreateDocument(DemoUsers.ChidiId, "Shared", DateTimeOffset.UtcNow);
        await fixture.Documents.CreateAsync(owned, CancellationToken.None);
        await fixture.Documents.CreateAsync(shared, CancellationToken.None);
        await ShareAsync(shared.Id, DemoUsers.AminaId, DemoUsers.ChidiId);

        var rows = await fixture.Documents.ListAsync(DemoUsers.AminaId,
            DocumentScope.Owned, CancellationToken.None);

        Assert.True(rows.IsSuccess);
        var row = Assert.Single(rows.Value);
        Assert.Equal(owned.Id, row.Id);
        Assert.True(row.IsOwner);
    }

    [Fact]
    public async Task All_scope_orders_accessible_documents_by_updated_time_then_id()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var older = CreateDocument(DemoUsers.AminaId, "Older", timestamp.AddMinutes(-1));
        var firstAtSameTime = CreateDocument(DemoUsers.AminaId, "First", timestamp,
            Guid.Parse("10000000-0000-0000-0000-000000000000"));
        var secondAtSameTime = CreateDocument(DemoUsers.ChidiId, "Second", timestamp,
            Guid.Parse("20000000-0000-0000-0000-000000000000"));
        await fixture.Documents.CreateAsync(older, CancellationToken.None);
        await fixture.Documents.CreateAsync(firstAtSameTime, CancellationToken.None);
        await fixture.Documents.CreateAsync(secondAtSameTime, CancellationToken.None);
        await ShareAsync(secondAtSameTime.Id, DemoUsers.AminaId, DemoUsers.ChidiId);

        var rows = await fixture.Documents.ListAsync(DemoUsers.AminaId,
            DocumentScope.All, CancellationToken.None);

        Assert.True(rows.IsSuccess);
        Assert.Equal(new[] { firstAtSameTime.Id, secondAtSameTime.Id, older.Id },
            rows.Value.Select(row => row.Id));
        Assert.Equal(new[] { true, false, true }, rows.Value.Select(row => row.IsOwner));
    }

    [Fact]
    public async Task Collaborator_can_get_shared_document_with_edit_only_capability()
    {
        var document = CreateDocument(DemoUsers.AminaId, "Shared draft", DateTimeOffset.UtcNow);
        await fixture.Documents.CreateAsync(document, CancellationToken.None);
        await ShareAsync(document.Id, DemoUsers.ChidiId, DemoUsers.AminaId);

        var result = await fixture.Documents.GetAsync(DemoUsers.ChidiId, document.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsOwner);
        Assert.True(result.Value.CanEdit);
        Assert.False(result.Value.CanRename);
        Assert.False(result.Value.CanShare);
        Assert.False(result.Value.CanDelete);
    }

    [Fact]
    public async Task Inaccessible_get_returns_not_found()
    {
        var document = CreateDocument(DemoUsers.AminaId, "Private", DateTimeOffset.UtcNow);
        await fixture.Documents.CreateAsync(document, CancellationToken.None);

        var result = await fixture.Documents.GetAsync(DemoUsers.TayoId, document.Id,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.Error.Code);
    }

    [Fact]
    public async Task Unknown_document_get_is_concealed_as_not_found()
    {
        var result = await fixture.Documents.GetAsync(DemoUsers.AminaId, Guid.NewGuid(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.Error.Code);
    }

    [Fact]
    public async Task Create_returns_the_single_persisted_owner_projection()
    {
        var document = CreateDocument(DemoUsers.AminaId, "Atomic create", DateTimeOffset.UtcNow);

        var result = await fixture.Documents.CreateAsync(document, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(document.Id, result.Value.Id);
        Assert.Equal("Amina Okafor", result.Value.Owner.DisplayName);
        await using var connection = await fixture.OpenConnectionAsync();
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM documents WHERE id = @Id", new { document.Id }));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => fixture.ResetAsync();

    private async Task ShareAsync(Guid documentId, Guid userId, Guid sharedByUserId)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO document_shares (document_id, user_id, shared_by_user_id, created_at)
            VALUES (@DocumentId, @UserId, @SharedByUserId, now())
            """, new { DocumentId = documentId, UserId = userId, SharedByUserId = sharedByUserId });
    }

    private static Document CreateDocument(Guid ownerId, string title, DateTimeOffset updatedAt,
        Guid? id = null) => new(id ?? Guid.NewGuid(), ownerId, title, ContentFormat.Lexical,
            "{\"root\":{}}", title, 1, updatedAt.AddMinutes(-1), updatedAt);
}
