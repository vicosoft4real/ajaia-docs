using System.Text;
using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Application.Features.Documents.Delete;
using AjaiaDocs.Application.Features.Documents.Rename;
using AjaiaDocs.Application.Features.Documents.UpdateContent;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using NSubstitute;

namespace AjaiaDocs.UnitTests.Application;

public sealed class DocumentWriteHandlerTests
{
    private static readonly Guid ActorId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid DocumentId = Guid.Parse("00000000-0000-0000-0000-000000000010");

    [Fact]
    public async Task Content_update_forwards_valid_lexical_state_to_the_repository()
    {
        var repository = Substitute.For<IDocumentRepository>();
        repository.UpdateContentAsync(ActorId, DocumentId, DocumentContentDefaults.EmptyLexical,
                "Edited", ContentFormat.Lexical, 4, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentDto>.Success(Dto(version: 5)));
        var handler = new UpdateDocumentContentHandler(repository,
            new UpdateDocumentContentValidator());

        var result = await handler.HandleAsync(ActorId, DocumentId,
            new UpdateDocumentContentCommand("lexical", DocumentContentDefaults.EmptyLexical,
                "Edited", 4), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Version);
        await repository.Received(1).UpdateContentAsync(ActorId, DocumentId,
            DocumentContentDefaults.EmptyLexical, "Edited", ContentFormat.Lexical, 4,
            CancellationToken.None);
    }

    [Fact]
    public async Task Content_at_exact_utf8_byte_limit_is_accepted()
    {
        var content = new string('\u00e9', Document.MaxContentBytes / 2);
        Assert.Equal(Document.MaxContentBytes, Encoding.UTF8.GetByteCount(content));
        var repository = Substitute.For<IDocumentRepository>();
        repository.UpdateContentAsync(ActorId, DocumentId, content, string.Empty,
                ContentFormat.PlainText, 1, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentDto>.Success(Dto(version: 2)));
        var handler = new UpdateDocumentContentHandler(repository,
            new UpdateDocumentContentValidator());

        var result = await handler.HandleAsync(ActorId, DocumentId,
            new UpdateDocumentContentCommand("plainText", content, string.Empty, 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        await repository.Received(1).UpdateContentAsync(ActorId, DocumentId, content,
            string.Empty, ContentFormat.PlainText, 1, CancellationToken.None);
    }

    [Fact]
    public async Task Content_over_utf8_byte_limit_is_rejected_before_the_repository()
    {
        var content = new string('\u00e9', Document.MaxContentBytes / 2) + "a";
        Assert.Equal(Document.MaxContentBytes + 1, Encoding.UTF8.GetByteCount(content));
        var repository = Substitute.For<IDocumentRepository>();
        var handler = new UpdateDocumentContentHandler(repository,
            new UpdateDocumentContentValidator());

        var result = await handler.HandleAsync(ActorId, DocumentId,
            new UpdateDocumentContentCommand("plainText", content, string.Empty, 1),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("content_too_large", result.Error.Code);
        await repository.DidNotReceive().UpdateContentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ContentFormat>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    public static TheoryData<string> InvalidLexicalStates => new()
    {
        "{\"root\":{\"version\":1,\"children\":[]}}",
        "{\"root\":{\"type\":\"root\",\"version\":1}}",
        "{\"root\":{\"type\":\"root\",\"version\":1,\"children\":{}}}",
        "{\"root\":{\"type\":\"root\",\"version\":0,\"children\":[]}}",
        "{malformed"
    };

    [Theory]
    [MemberData(nameof(InvalidLexicalStates))]
    public async Task Invalid_lexical_state_is_rejected_before_the_repository(string content)
    {
        var repository = Substitute.For<IDocumentRepository>();
        var handler = new UpdateDocumentContentHandler(repository,
            new UpdateDocumentContentValidator());

        var result = await handler.HandleAsync(ActorId, DocumentId,
            new UpdateDocumentContentCommand("lexical", content, "Edited", 1),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_editor_state", result.Error.Code);
        await repository.DidNotReceive().UpdateContentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ContentFormat>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rename_trims_valid_title_and_forwards_expected_version()
    {
        var repository = Substitute.For<IDocumentRepository>();
        repository.RenameAsync(ActorId, DocumentId, "Launch brief", 3,
                Arg.Any<CancellationToken>())
            .Returns(Result<DocumentDto>.Success(Dto(version: 4)));
        var handler = new RenameDocumentHandler(repository, new RenameDocumentValidator());

        var result = await handler.HandleAsync(ActorId, DocumentId,
            new RenameDocumentCommand("  Launch brief  ", 3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await repository.Received(1).RenameAsync(ActorId, DocumentId, "Launch brief", 3,
            CancellationToken.None);
    }

    [Fact]
    public async Task Invalid_rename_is_rejected_before_the_repository()
    {
        var repository = Substitute.For<IDocumentRepository>();
        var handler = new RenameDocumentHandler(repository, new RenameDocumentValidator());

        var result = await handler.HandleAsync(ActorId, DocumentId,
            new RenameDocumentCommand("   ", 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("title_required", result.Error.Code);
        await repository.DidNotReceive().RenameAsync(Arg.Any<Guid>(), Arg.Any<Guid>(),
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_forwards_the_command_document_id()
    {
        var repository = Substitute.For<IDocumentRepository>();
        repository.DeleteAsync(ActorId, DocumentId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));
        var handler = new DeleteDocumentHandler(repository);

        var result = await handler.HandleAsync(ActorId, new DeleteDocumentCommand(DocumentId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        await repository.Received(1).DeleteAsync(ActorId, DocumentId, CancellationToken.None);
    }

    private static DocumentDto Dto(int version) => new(DocumentId, ActorId, "Title", "lexical",
        DocumentContentDefaults.EmptyLexical, string.Empty, version, DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow, new UserSummaryDto(ActorId, "Actor", "actor@example.com", "#365CF5"),
        true, true, true, true, true);
}
