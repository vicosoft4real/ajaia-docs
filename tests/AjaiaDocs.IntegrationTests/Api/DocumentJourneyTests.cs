using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

    [Fact]
    public async Task Malformed_json_and_unsupported_media_are_sanitized_problems()
    {
        var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);

        var malformed = await SendRawWithAntiforgeryAsync(owner, "{not-json",
            "application/json");
        var unsupported = await SendRawWithAntiforgeryAsync(owner, "{}", "text/plain");

        await AssertBindingProblemAsync(malformed, HttpStatusCode.BadRequest);
        await AssertBindingProblemAsync(unsupported, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Request_body_cannot_spoof_the_cookie_actor()
    {
        var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var created = await owner.PostJsonWithAntiforgeryAsync<DocumentDto>(
            "/api/documents", new
            {
                title = "Cookie-owned",
                ownerId = DemoUsers.ChidiId,
                actorId = DemoUsers.TayoId
            });

        Assert.Equal(DemoUsers.AminaId, created.OwnerId);
        Assert.True(created.IsOwner);
    }

    public static TheoryData<HttpMethod, string, Func<HttpContent?>> MutationsWithoutTokens => new()
    {
        { HttpMethod.Delete, "/api/session", () => null },
        { HttpMethod.Post, "/api/documents", () => JsonContent.Create(new { title = "x" }) },
        { HttpMethod.Post, "/api/documents/import", CreateFileContent },
        { HttpMethod.Put, $"/api/documents/{Guid.Empty}/content", () => JsonContent.Create(new
            { contentFormat = "plainText", content = "x", plainText = "x", expectedVersion = 1 }) },
        { HttpMethod.Put, $"/api/documents/{Guid.Empty}/title", () => JsonContent.Create(new
            { title = "x", expectedVersion = 1 }) },
        { HttpMethod.Delete, $"/api/documents/{Guid.Empty}", () => null },
        { HttpMethod.Post, $"/api/documents/{Guid.Empty}/shares", () =>
            JsonContent.Create(new { userId = DemoUsers.ChidiId }) },
        { HttpMethod.Delete, $"/api/documents/{Guid.Empty}/shares/{DemoUsers.ChidiId}", () => null }
    };

    [Theory]
    [MemberData(nameof(MutationsWithoutTokens))]
    public async Task Every_authenticated_mutation_requires_antiforgery(HttpMethod method,
        string uri, Func<HttpContent?> createContent)
    {
        var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        using var request = new HttpRequestMessage(method, uri) { Content = createContent() };

        var response = await owner.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("antiforgery_validation_failed",
            (await response.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
    }

    private static async Task<HttpResponseMessage> SendRawWithAntiforgeryAsync(HttpClient client,
        string body, string mediaType)
    {
        using var tokenResponse = await client.GetAsync("/api/session/antiforgery");
        using var tokenBody = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/documents")
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType)
        };
        request.Headers.Add("X-XSRF-TOKEN",
            tokenBody.RootElement.GetProperty("token").GetString());
        return await client.SendAsync(request);
    }

    private static async Task AssertBindingProblemAsync(HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.Content.Headers.ContentType is not null,
            $"Expected problem content for {(int)response.StatusCode}; body: '{raw}'.");
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);
        var problem = JsonSerializer.Deserialize<ProblemResponse>(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal("invalid_request", problem!.Code);
        Assert.Equal("The request is invalid.", problem.Detail);
        Assert.DoesNotContain("JsonException", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CreateDocumentRequest", raw, StringComparison.Ordinal);
    }

    private static HttpContent CreateFileContent()
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent([1]), "file", "one.txt");
        return form;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await postgres.ResetAsync();
    }
}
