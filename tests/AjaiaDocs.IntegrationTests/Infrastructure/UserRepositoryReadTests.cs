using AjaiaDocs.Core.Documents;
using Dapper;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.CollectionName)]
public sealed class UserRepositoryReadTests(PostgresFixture fixture) : IAsyncLifetime
{
    [Fact]
    public async Task Get_seeded_returns_stable_user_and_conceals_unknown_user()
    {
        var found = await fixture.Users.GetSeededAsync(DemoUsers.AminaId,
            CancellationToken.None);
        var unknown = await fixture.Users.GetSeededAsync(Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal("Amina Okafor", found.Value.DisplayName);
        Assert.Equal("amina@example.test", found.Value.Email);
        Assert.False(unknown.IsSuccess);
        Assert.Equal("not_found", unknown.Error.Code);
    }

    [Fact]
    public async Task Unknown_document_candidates_are_concealed_as_not_found()
    {
        var result = await fixture.Users.ListShareCandidatesAsync(DemoUsers.AminaId,
            Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.Error.Code);
    }

    [Fact]
    public async Task Inaccessible_document_candidates_are_concealed_as_not_found()
    {
        var document = CreateDocument(DemoUsers.AminaId);
        await fixture.Documents.CreateAsync(document, CancellationToken.None);

        var result = await fixture.Users.ListShareCandidatesAsync(DemoUsers.TayoId,
            document.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("not_found", result.Error.Code);
    }

    [Fact]
    public async Task Collaborator_candidates_require_document_owner()
    {
        var document = CreateDocument(DemoUsers.AminaId);
        await fixture.Documents.CreateAsync(document, CancellationToken.None);
        await ShareAsync(document.Id, DemoUsers.ChidiId, DemoUsers.AminaId);

        var result = await fixture.Users.ListShareCandidatesAsync(DemoUsers.ChidiId,
            document.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("owner_required", result.Error.Code);
    }

    [Fact]
    public async Task Owner_candidates_exclude_owner_and_existing_grants()
    {
        var document = CreateDocument(DemoUsers.AminaId);
        await fixture.Documents.CreateAsync(document, CancellationToken.None);
        await ShareAsync(document.Id, DemoUsers.ChidiId, DemoUsers.AminaId);

        var result = await fixture.Users.ListShareCandidatesAsync(DemoUsers.AminaId,
            document.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var candidate = Assert.Single(result.Value);
        Assert.Equal(DemoUsers.TayoId, candidate.Id);
        Assert.Equal("Tayo Bello", candidate.DisplayName);
        Assert.Equal("tayo@example.test", candidate.Email);
        Assert.Equal("#C77A15", candidate.AvatarColor);
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

    private static Document CreateDocument(Guid ownerId) => new(Guid.NewGuid(), ownerId,
        "Candidate authorization", ContentFormat.PlainText, "", "", 1,
        DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
