using System.Net;
using System.Net.Http.Json;
using System.Text;
using AjaiaDocs.Api.Common;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Application.Features.Import;
using AjaiaDocs.IntegrationTests.Infrastructure;

namespace AjaiaDocs.IntegrationTests.Api;

[Collection(PostgresCollection.CollectionName)]
public sealed class ImportEndpointsTests(PostgresFixture postgres) : IAsyncLifetime
{
    private readonly AjaiaDocsWebApplicationFactory _factory = new(postgres);

    [Theory]
    [InlineData("review.txt", "plainText")]
    [InlineData("review.md", "markdown")]
    public async Task Valid_import_is_persisted_before_the_response(string fileName,
        string expectedFormat)
    {
        var client = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var response = await client.PostFileWithAntiforgeryAsync(fileName,
            Encoding.UTF8.GetBytes("# Review\n\n- persisted"));
        var imported = await response.Content.ReadFromJsonAsync<DocumentDto>();
        var persisted = await client.GetFromJsonAsync<DocumentDto>(
            $"/api/documents/{imported!.Id}");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(expectedFormat, imported.ContentFormat);
        Assert.Equal(imported, persisted);
    }

    [Theory]
    [InlineData("review.pdf", "unsupported_file_type")]
    [InlineData("review.txt", "invalid_utf8")]
    public async Task Invalid_extension_and_utf8_are_rejected(string fileName,
        string expectedCode)
    {
        var bytes = expectedCode == "invalid_utf8"
            ? new byte[] { 0xC3, 0x28 }
            : Encoding.UTF8.GetBytes("nope");
        var client = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var response = await client.PostFileWithAntiforgeryAsync(fileName, bytes);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedCode,
            (await response.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
    }

    [Fact]
    public async Task File_over_one_mebibyte_is_rejected_at_the_transport_boundary()
    {
        var client = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
        var response = await client.PostFileWithAntiforgeryAsync("large.txt",
            new byte[StrictTextImportParser.MaxFileBytes + 1]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("file_too_large",
            (await response.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
        Assert.Empty((await client.GetFromJsonAsync<List<DocumentListItemDto>>(
            "/api/documents?scope=all"))!);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await postgres.ResetAsync();
    }
}
