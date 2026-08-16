using AjaiaDocs.Application.Common;
using AjaiaDocs.Core.Documents;
using Dapper;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.CollectionName)]
public sealed class DocumentRepositoryWriteTests(PostgresFixture fixture) : IAsyncLifetime
{
    private Guid _documentId;

    [Fact]
    public async Task Stale_content_update_returns_conflict_without_overwrite()
    {
        var first = await fixture.Documents.UpdateContentAsync(DemoUsers.AminaId,
            _documentId, DocumentContentDefaults.EmptyLexical, "first", ContentFormat.Lexical, 1,
            CancellationToken.None);
        var stale = await fixture.Documents.UpdateContentAsync(DemoUsers.AminaId,
            _documentId, DocumentContentDefaults.EmptyLexical, "stale", ContentFormat.Lexical, 1,
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(stale.IsSuccess);
        Assert.Equal("conflict", stale.Error.Code);
        Assert.Equal("first", (await fixture.Documents.GetAsync(
            DemoUsers.AminaId, _documentId, CancellationToken.None)).Value.PlainText);
    }

    [Fact]
    public async Task Collaborator_can_update_content_with_the_current_version()
    {
        await ShareAsync(DemoUsers.ChidiId);

        var result = await fixture.Documents.UpdateContentAsync(DemoUsers.ChidiId,
            _documentId, DocumentContentDefaults.EmptyLexical, "Collaborative edit",
            ContentFormat.Lexical, 1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Version);
        Assert.Equal("Collaborative edit", result.Value.PlainText);
        Assert.False(result.Value.IsOwner);
    }

    [Fact]
    public async Task Inaccessible_content_update_is_concealed_as_not_found()
    {
        var result = await fixture.Documents.UpdateContentAsync(DemoUsers.TayoId,
            _documentId, DocumentContentDefaults.EmptyLexical, "Private edit",
            ContentFormat.Lexical, 1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.Error.Code);
    }

    [Fact]
    public async Task Owner_rename_increments_version_and_trims_title_from_the_handler_boundary()
    {
        var result = await fixture.Documents.RenameAsync(DemoUsers.AminaId, _documentId,
            "Renamed", 1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Renamed", result.Value.Title);
        Assert.Equal(2, result.Value.Version);
    }

    [Fact]
    public async Task Stale_owner_rename_returns_conflict_without_overwrite()
    {
        var first = await fixture.Documents.RenameAsync(DemoUsers.AminaId, _documentId,
            "Current", 1, CancellationToken.None);
        var stale = await fixture.Documents.RenameAsync(DemoUsers.AminaId, _documentId,
            "Stale", 1, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(stale.IsSuccess);
        Assert.Equal("conflict", stale.Error.Code);
        Assert.Equal("Current", (await fixture.Documents.GetAsync(DemoUsers.AminaId,
            _documentId, CancellationToken.None)).Value.Title);
    }

    [Fact]
    public async Task Collaborator_rename_returns_owner_required_without_overwrite()
    {
        await ShareAsync(DemoUsers.ChidiId);

        var result = await fixture.Documents.RenameAsync(DemoUsers.ChidiId, _documentId,
            "Forbidden", 1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("owner_required", result.Error.Code);
        Assert.Equal("Original", (await fixture.Documents.GetAsync(DemoUsers.AminaId,
            _documentId, CancellationToken.None)).Value.Title);
    }

    [Fact]
    public async Task Collaborator_delete_returns_owner_required_and_preserves_document()
    {
        await ShareAsync(DemoUsers.ChidiId);

        var result = await fixture.Documents.DeleteAsync(DemoUsers.ChidiId, _documentId,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("owner_required", result.Error.Code);
        Assert.True((await fixture.Documents.GetAsync(DemoUsers.AminaId, _documentId,
            CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task Owner_delete_removes_document_and_cascades_shares()
    {
        await ShareAsync(DemoUsers.ChidiId);

        var result = await fixture.Documents.DeleteAsync(DemoUsers.AminaId, _documentId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        await using var connection = await fixture.OpenConnectionAsync();
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM document_shares WHERE document_id = @DocumentId",
            new { DocumentId = _documentId }));
    }

    [Fact]
    public async Task Inaccessible_delete_is_concealed_as_not_found()
    {
        var result = await fixture.Documents.DeleteAsync(DemoUsers.TayoId, _documentId,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.Error.Code);
    }

    public async Task InitializeAsync()
    {
        _documentId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var document = new Document(_documentId, DemoUsers.AminaId, "Original",
            ContentFormat.Lexical, DocumentContentDefaults.EmptyLexical, string.Empty, 1,
            now.AddMinutes(-1), now);
        await fixture.Documents.CreateAsync(document, CancellationToken.None);
    }

    public Task DisposeAsync() => fixture.ResetAsync();

    private async Task ShareAsync(Guid userId)
    {
        await using var connection = await fixture.OpenConnectionAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO document_shares (document_id, user_id, shared_by_user_id, created_at)
            VALUES (@DocumentId, @UserId, @SharedByUserId, now())
            """, new
            {
                DocumentId = _documentId,
                UserId = userId,
                SharedByUserId = DemoUsers.AminaId
            });
    }
}
