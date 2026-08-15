using System.Text;
using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.UnitTests.Core;

public sealed class DocumentTests
{
    private static readonly Guid DocumentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_trims_title_and_starts_at_version_one()
    {
        var result = Document.Create(
            DocumentId,
            OwnerId,
            "  Review notes  ",
            ContentFormat.Lexical,
            "{\"root\":{\"children\":[]}}",
            string.Empty,
            Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Review notes", result.Value.Title);
        Assert.Equal(1, result.Value.Version);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_title(string title)
    {
        var result = Document.Create(DocumentId, OwnerId, title,
            ContentFormat.Lexical, "{}", string.Empty, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal("title_required", result.Error.Code);
    }

    [Fact]
    public void Create_accepts_a_title_at_the_maximum_length()
    {
        var result = Document.Create(DocumentId, OwnerId, new string('a', 120),
            ContentFormat.Markdown, "# Title", "Title", Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_rejects_a_title_over_the_maximum_length()
    {
        var result = Document.Create(DocumentId, OwnerId, new string('a', 121),
            ContentFormat.Markdown, "# Title", "Title", Now);

        Assert.False(result.IsSuccess);
        Assert.Equal("title_too_long", result.Error.Code);
    }

    [Fact]
    public void Rename_updates_a_document_when_the_version_matches()
    {
        var document = CreateDocument();

        var result = document.Rename("  Renamed  ", document.Version, Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("Renamed", result.Value.Title);
        Assert.Equal(2, result.Value.Version);
        Assert.Equal(Now.AddMinutes(1), result.Value.UpdatedAt);
    }

    [Fact]
    public void Rename_rejects_a_stale_version()
    {
        var document = CreateDocument();

        var result = document.Rename("Renamed", document.Version + 1, Now.AddMinutes(1));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.Error.Code);
    }

    [Fact]
    public void UpdateContent_updates_a_document_when_the_version_matches()
    {
        var document = CreateDocument();

        var result = document.UpdateContent("# Updated", "Updated", ContentFormat.Markdown,
            document.Version, Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("# Updated", result.Value.Content);
        Assert.Equal(ContentFormat.Markdown, result.Value.ContentFormat);
        Assert.Equal(2, result.Value.Version);
    }

    [Fact]
    public void UpdateContent_rejects_a_stale_version()
    {
        var document = CreateDocument();

        var result = document.UpdateContent("# Updated", "Updated", ContentFormat.Markdown,
            document.Version + 1, Now.AddMinutes(1));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.Error.Code);
    }

    [Fact]
    public void UpdateContent_rejects_content_larger_than_two_megabytes()
    {
        var document = CreateDocument();
        var content = new string('a', 2 * 1024 * 1024 + 1);

        var result = document.UpdateContent(content, string.Empty, ContentFormat.PlainText,
            document.Version, Now.AddMinutes(1));

        Assert.False(result.IsSuccess);
        Assert.Equal("content_too_large", result.Error.Code);
    }

    [Fact]
    public void UpdateContent_rejects_an_unknown_content_format()
    {
        var document = CreateDocument();

        var result = document.UpdateContent("content", "content", (ContentFormat)99,
            document.Version, Now.AddMinutes(1));

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_content_format", result.Error.Code);
    }

    [Fact]
    public void Create_counts_content_size_in_utf8_bytes()
    {
        var content = new string('€', (2 * 1024 * 1024 / Encoding.UTF8.GetByteCount("€")) + 1);

        var result = Document.Create(DocumentId, OwnerId, "Title", ContentFormat.PlainText,
            content, content, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal("content_too_large", result.Error.Code);
    }

    private static Document CreateDocument() => Document.Create(DocumentId, OwnerId, "Original",
        ContentFormat.Lexical, "{}", string.Empty, Now).Value;
}
