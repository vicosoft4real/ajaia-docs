using AjaiaDocs.Core.Documents;
using Dapper;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.CollectionName)]
public sealed class DocumentShareRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private Guid _documentId;

    [Fact]
    public async Task Owner_can_grant_and_list_a_seeded_collaborator()
    {
        var now = new DateTimeOffset(2026, 8, 16, 10, 0, 0, TimeSpan.Zero);

        var grant = await fixture.Shares.GrantAsync(DemoUsers.AminaId, _documentId,
            DemoUsers.ChidiId, now, CancellationToken.None);
        var list = await fixture.Shares.ListAsync(DemoUsers.AminaId, _documentId,
            CancellationToken.None);

        Assert.True(grant.IsSuccess);
        Assert.Equal(_documentId, grant.Value.DocumentId);
        Assert.Equal(DemoUsers.ChidiId, grant.Value.UserId);
        Assert.Equal("Chidi Okeke", grant.Value.DisplayName);
        Assert.Equal("chidi@example.test", grant.Value.Email);
        Assert.Equal("#25A77A", grant.Value.AvatarColor);
        Assert.Equal(now, grant.Value.CreatedAt);
        Assert.Equal(grant.Value, Assert.Single(list.Value));
    }

    [Fact]
    public async Task Duplicate_grant_has_a_stable_conflict()
    {
        await GrantChidiAsync();

        var result = await GrantChidiAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("duplicate_share", result.Error.Code);
    }

    [Fact]
    public async Task Owner_self_share_trigger_has_a_stable_validation_error()
    {
        var result = await fixture.Shares.GrantAsync(DemoUsers.AminaId, _documentId,
            DemoUsers.AminaId, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("self_share", result.Error.Code);
    }

    [Fact]
    public async Task Unknown_target_user_has_a_stable_validation_error()
    {
        var result = await fixture.Shares.GrantAsync(DemoUsers.AminaId, _documentId,
            Guid.NewGuid(), DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("user_not_found", result.Error.Code);
    }

    [Fact]
    public async Task Collaborator_owner_only_share_attempts_return_owner_required()
    {
        await GrantChidiAsync();

        var list = await fixture.Shares.ListAsync(DemoUsers.ChidiId, _documentId,
            CancellationToken.None);
        var grant = await fixture.Shares.GrantAsync(DemoUsers.ChidiId, _documentId,
            DemoUsers.TayoId, DateTimeOffset.UtcNow, CancellationToken.None);
        var revoke = await fixture.Shares.RevokeAsync(DemoUsers.ChidiId, _documentId,
            DemoUsers.ChidiId, CancellationToken.None);

        Assert.Equal("owner_required", list.Error.Code);
        Assert.Equal("owner_required", grant.Error.Code);
        Assert.Equal("owner_required", revoke.Error.Code);
    }

    [Fact]
    public async Task Inaccessible_document_is_concealed_for_all_share_operations()
    {
        var list = await fixture.Shares.ListAsync(DemoUsers.TayoId, _documentId,
            CancellationToken.None);
        var grant = await fixture.Shares.GrantAsync(DemoUsers.TayoId, _documentId,
            DemoUsers.ChidiId, DateTimeOffset.UtcNow, CancellationToken.None);
        var revoke = await fixture.Shares.RevokeAsync(DemoUsers.TayoId, _documentId,
            DemoUsers.ChidiId, CancellationToken.None);

        Assert.Equal("not_found", list.Error.Code);
        Assert.Equal("not_found", grant.Error.Code);
        Assert.Equal("not_found", revoke.Error.Code);
    }

    [Fact]
    public async Task Unknown_document_is_concealed_for_all_share_operations()
    {
        var missingId = Guid.NewGuid();

        var list = await fixture.Shares.ListAsync(DemoUsers.AminaId, missingId,
            CancellationToken.None);
        var grant = await fixture.Shares.GrantAsync(DemoUsers.AminaId, missingId,
            DemoUsers.ChidiId, DateTimeOffset.UtcNow, CancellationToken.None);
        var revoke = await fixture.Shares.RevokeAsync(DemoUsers.AminaId, missingId,
            DemoUsers.ChidiId, CancellationToken.None);

        Assert.Equal("not_found", list.Error.Code);
        Assert.Equal("not_found", grant.Error.Code);
        Assert.Equal("not_found", revoke.Error.Code);
    }

    [Fact]
    public async Task Owner_can_revoke_an_existing_share()
    {
        await GrantChidiAsync();

        var result = await fixture.Shares.RevokeAsync(DemoUsers.AminaId, _documentId,
            DemoUsers.ChidiId, CancellationToken.None);
        var shares = await fixture.Shares.ListAsync(DemoUsers.AminaId, _documentId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Empty(shares.Value);
    }

    [Fact]
    public async Task Revoking_a_missing_grant_has_a_stable_not_found_error()
    {
        var result = await fixture.Shares.RevokeAsync(DemoUsers.AminaId, _documentId,
            DemoUsers.ChidiId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("share_not_found", result.Error.Code);
    }

    [Fact]
    public async Task Deleting_document_cascades_repository_grants()
    {
        await GrantChidiAsync();

        var deleted = await fixture.Documents.DeleteAsync(DemoUsers.AminaId, _documentId,
            CancellationToken.None);

        Assert.True(deleted.IsSuccess);
        await using var connection = await fixture.OpenConnectionAsync();
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM document_shares WHERE document_id = @DocumentId",
            new { DocumentId = _documentId }));
    }

    public async Task InitializeAsync()
    {
        _documentId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        await fixture.Documents.CreateAsync(new Document(_documentId, DemoUsers.AminaId,
            "Share repository", ContentFormat.PlainText, string.Empty, string.Empty, 1,
            now, now), CancellationToken.None);
    }

    public Task DisposeAsync() => fixture.ResetAsync();

    private Task<AjaiaDocs.Core.Common.Result<AjaiaDocs.Application.Features.Sharing.DocumentShareDto>>
        GrantChidiAsync() => fixture.Shares.GrantAsync(DemoUsers.AminaId, _documentId,
            DemoUsers.ChidiId, DateTimeOffset.UtcNow, CancellationToken.None);
}
