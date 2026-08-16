using System.Net;
using System.Net.Http.Json;
using AjaiaDocs.Api.Common;
using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Application.Features.Sharing;
using AjaiaDocs.IntegrationTests.Infrastructure;

namespace AjaiaDocs.IntegrationTests.Api;

[Collection(PostgresCollection.CollectionName)]
public sealed class DocumentJourneyTests(PostgresFixture postgres) : IAsyncLifetime
{
    private readonly AjaiaDocsWebApplicationFactory _factory = new(postgres);

    [Fact]
    public async Task Owner_can_share_and_collaborator_can_edit_but_not_rename()
    {
        var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var created = await owner.PostJsonWithAntiforgeryAsync<DocumentDto>(
            "/api/documents", new { title = "Launch brief" });
        await owner.PostJsonWithAntiforgeryAsync<DocumentShareDto>(
            $"/api/documents/{created.Id}/shares", new { userId = DemoUsers.ChidiId });

        var collaborator = await _factory.CreateAuthenticatedClientAsync(DemoUsers.ChidiId);
        var shared = await collaborator.GetFromJsonAsync<List<DocumentListItemDto>>(
            "/api/documents?scope=shared");
        Assert.Contains(shared!, item => item.Id == created.Id && !item.IsOwner);

        var edited = await collaborator.PutJsonWithAntiforgeryAsync<DocumentDto>(
            $"/api/documents/{created.Id}/content",
            new { contentFormat = "lexical", content = DocumentContentDefaults.EmptyLexical,
                plainText = "Edited", expectedVersion = created.Version });
        Assert.Equal(created.Version + 1, edited.Version);

        var rename = await collaborator.PutWithAntiforgeryAsync(
            $"/api/documents/{created.Id}/title",
            new { title = "Forbidden", expectedVersion = edited.Version });
        Assert.Equal(HttpStatusCode.Forbidden, rename.StatusCode);
        Assert.Equal("owner_required",
            (await rename.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
    }

    [Fact]
    public async Task Anonymous_missing_antiforgery_unknown_and_invalid_scope_are_stable_problems()
    {
        var anonymous = _factory.CreateClient();
        var anonymousGet = await anonymous.GetAsync("/api/documents");

        var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var missingToken = await owner.PostAsJsonAsync("/api/documents", new { title = "No" });
        var unknown = await owner.GetAsync($"/api/documents/{Guid.NewGuid()}");
        var invalidScope = await owner.GetAsync("/api/documents?scope=elsewhere");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousGet.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidScope.StatusCode);
        Assert.All(new[] { missingToken, unknown, invalidScope }, response =>
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType));
    }

    [Fact]
    public async Task Stale_content_update_returns_conflict_and_does_not_overwrite()
    {
        var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var created = await owner.PostJsonWithAntiforgeryAsync<DocumentDto>(
            "/api/documents", new { title = "Versioned" });
        var first = await owner.PutJsonWithAntiforgeryAsync<DocumentDto>(
            $"/api/documents/{created.Id}/content",
            new { contentFormat = "plainText", content = "Saved", plainText = "Saved",
                expectedVersion = created.Version });

        var stale = await owner.PutWithAntiforgeryAsync(
            $"/api/documents/{created.Id}/content",
            new { contentFormat = "plainText", content = "Lost", plainText = "Lost",
                expectedVersion = created.Version });
        var persisted = await owner.GetFromJsonAsync<DocumentDto>(
            $"/api/documents/{created.Id}");

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("conflict", (await stale.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
        Assert.Equal(first.Version, persisted!.Version);
        Assert.Equal("Saved", persisted.Content);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await postgres.ResetAsync();
    }
}
