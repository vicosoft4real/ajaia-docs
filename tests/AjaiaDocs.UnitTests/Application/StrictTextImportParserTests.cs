using System.Text;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Application.Features.Import;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using AjaiaDocs.UnitTests.Fixtures;
using NSubstitute;

namespace AjaiaDocs.UnitTests.Application;

public sealed class StrictTextImportParserTests
{
    [Fact]
    public void Invalid_utf8_is_rejected()
    {
        var result = StrictTextImportParser.Parse("broken.md", [0xC3, 0x28]);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_utf8", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("notes.txt", ContentFormat.PlainText)]
    [InlineData("NOTES.MD", ContentFormat.Markdown)]
    public void Supported_file_is_persistable(string fileName, ContentFormat format)
    {
        var result = StrictTextImportParser.Parse(fileName,
            Encoding.UTF8.GetBytes("# Shared plan"));

        Assert.True(result.IsSuccess);
        Assert.Equal(format, result.Value.Format);
        Assert.Equal("notes", result.Value.Title, ignoreCase: true);
        Assert.Equal("# Shared plan", result.Value.Content);
        Assert.Equal("# Shared plan", result.Value.PlainText);
    }

    [Fact]
    public void File_at_exactly_one_mib_is_accepted()
    {
        var result = StrictTextImportParser.Parse("boundary.txt",
            new byte[StrictTextImportParser.MaxFileBytes]);

        Assert.True(result.IsSuccess);
        Assert.Equal(StrictTextImportParser.MaxFileBytes, result.Value.Content.Length);
    }

    [Fact]
    public void File_over_one_mib_is_rejected_before_decoding()
    {
        var bytes = new byte[StrictTextImportParser.MaxFileBytes + 1];
        bytes[^1] = 0xFF;

        var result = StrictTextImportParser.Parse("oversized.txt", bytes);

        Assert.False(result.IsSuccess);
        Assert.Equal("file_too_large", result.Error.Code);
    }

    [Fact]
    public void Whitespace_only_file_is_allowed()
    {
        var result = StrictTextImportParser.Parse(" blank.md ",
            Encoding.UTF8.GetBytes(" \r\n\t"));

        Assert.True(result.IsSuccess);
        Assert.Equal("blank", result.Value.Title);
        Assert.Equal(" \r\n\t", result.Value.Content);
    }

    [Theory]
    [InlineData("archive.md.txt", "archive.md")]
    [InlineData("   .md", "Untitled document")]
    public void Title_is_derived_from_the_final_filename(string fileName, string expectedTitle)
    {
        var result = StrictTextImportParser.Parse(fileName, []);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTitle, result.Value.Title);
    }

    [Fact]
    public void Derived_title_is_trimmed_to_the_document_limit()
    {
        var fileName = $"  {new string('x', Document.MaxTitleLength + 20)}  .txt";

        var result = StrictTextImportParser.Parse(fileName, []);

        Assert.True(result.IsSuccess);
        Assert.Equal(Document.MaxTitleLength, result.Value.Title.Length);
        Assert.DoesNotContain(' ', result.Value.Title);
    }

    [Theory]
    [InlineData("notes")]
    [InlineData("notes.pdf")]
    [InlineData("notes.md.exe")]
    public void Unsupported_extension_is_rejected(string fileName)
    {
        var result = StrictTextImportParser.Parse(fileName, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("unsupported_file_type", result.Error.Code);
    }

    [Fact]
    public async Task Import_is_persisted_before_the_handler_returns()
    {
        var actorId = Guid.NewGuid();
        var persisted = new TaskCompletionSource<Document>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var repository = Substitute.For<IDocumentRepository>();
        repository.CreateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var document = call.Arg<Document>();
                persisted.SetResult(document);
                return Result<DocumentDto>.Success(DocumentFixtures.Dto(document, isOwner: true));
            });
        var handler = new ImportDocumentHandler(new StrictTextImportParser(), repository,
            TimeProvider.System);

        var result = await handler.HandleAsync(actorId, " plan.MD ",
            Encoding.UTF8.GetBytes("# Review"), CancellationToken.None);

        var stored = await persisted.Task;
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.Id, stored.Id);
        Assert.Equal(actorId, stored.OwnerId);
        Assert.Equal("plan", stored.Title);
        Assert.Equal(ContentFormat.Markdown, stored.ContentFormat);
        Assert.Equal("# Review", stored.Content);
        Assert.Equal("# Review", stored.PlainText);
        Assert.Equal(1, result.Value.Version);
    }

    [Fact]
    public async Task Invalid_import_is_not_persisted()
    {
        var repository = Substitute.For<IDocumentRepository>();
        var handler = new ImportDocumentHandler(new StrictTextImportParser(), repository,
            TimeProvider.System);

        var result = await handler.HandleAsync(Guid.NewGuid(), "plan.pdf",
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("unsupported_file_type", result.Error.Code);
        await repository.DidNotReceive().CreateAsync(Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
    }
}
