using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Application.Features.Documents.CreateDocument;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using AjaiaDocs.UnitTests.Fixtures;
using NSubstitute;

namespace AjaiaDocs.UnitTests.Application;

public sealed class CreateDocumentHandlerTests
{
    [Fact]
    public async Task Create_uses_cookie_actor_as_owner_and_returns_version_one()
    {
        var actorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var repository = Substitute.For<IDocumentRepository>();
        repository.CreateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>())
            .Returns(call => Result<DocumentDto>.Success(DocumentFixtures.Dto(
                call.Arg<Document>(), isOwner: true)));
        var handler = new CreateDocumentHandler(repository,
            new CreateDocumentValidator(), TimeProvider.System);

        var result = await handler.HandleAsync(actorId,
            new CreateDocumentCommand("  Sprint brief  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(actorId, result.Value.OwnerId);
        Assert.Equal("Sprint brief", result.Value.Title);
        Assert.Equal(1, result.Value.Version);
    }

    [Fact]
    public async Task Create_defaults_missing_title_and_initializes_an_empty_lexical_document()
    {
        var actorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var repository = Substitute.For<IDocumentRepository>();
        repository.CreateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>())
            .Returns(call => Result<DocumentDto>.Success(DocumentFixtures.Dto(
                call.Arg<Document>(), isOwner: true)));
        var handler = new CreateDocumentHandler(repository,
            new CreateDocumentValidator(), TimeProvider.System);

        var result = await handler.HandleAsync(actorId,
            new CreateDocumentCommand(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Untitled document", result.Value.Title);
        await repository.Received(1).CreateAsync(Arg.Is<Document>(document =>
            document.ContentFormat == ContentFormat.Lexical &&
            document.Content == DocumentContentDefaults.EmptyLexical &&
            document.PlainText == string.Empty), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_rejects_titles_over_the_document_limit_without_persisting()
    {
        var repository = Substitute.For<IDocumentRepository>();
        var handler = new CreateDocumentHandler(repository,
            new CreateDocumentValidator(), TimeProvider.System);

        var result = await handler.HandleAsync(Guid.NewGuid(),
            new CreateDocumentCommand(new string('a', Document.MaxTitleLength + 1)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("title_too_long", result.Error.Code);
        await repository.DidNotReceive().CreateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }
}
