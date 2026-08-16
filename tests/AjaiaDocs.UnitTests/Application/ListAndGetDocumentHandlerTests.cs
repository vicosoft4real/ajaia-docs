using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Application.Features.Documents.GetDocument;
using AjaiaDocs.Application.Features.Documents.ListDocuments;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using NSubstitute;

namespace AjaiaDocs.UnitTests.Application;

public sealed class ListAndGetDocumentHandlerTests
{
    [Fact]
    public async Task List_forwards_the_requested_scope_to_the_repository()
    {
        var actorId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var repository = Substitute.For<IDocumentRepository>();
        repository.ListAsync(actorId, DocumentScope.Shared, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<DocumentListItemDto>>.Success([]));
        var handler = new ListDocumentsHandler(repository);

        var result = await handler.HandleAsync(actorId,
            new ListDocumentsQuery(DocumentScope.Shared), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await repository.Received(1).ListAsync(actorId, DocumentScope.Shared,
            CancellationToken.None);
    }

    [Fact]
    public async Task Get_preserves_an_inaccessible_document_error()
    {
        var actorId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var documentId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        var inaccessible = new AjaiaError("not_found", "Document not found.", ErrorType.NotFound);
        var repository = Substitute.For<IDocumentRepository>();
        repository.GetAsync(actorId, documentId, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentDto>.Failure(inaccessible));
        var handler = new GetDocumentHandler(repository);

        var result = await handler.HandleAsync(actorId,
            new GetDocumentQuery(documentId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Same(inaccessible, result.Error);
    }
}
