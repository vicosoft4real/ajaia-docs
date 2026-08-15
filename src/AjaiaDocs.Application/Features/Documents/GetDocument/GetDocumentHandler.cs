using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Documents.GetDocument;

public sealed class GetDocumentHandler(IDocumentRepository repository)
{
    public Task<Result<DocumentDto>> HandleAsync(Guid actorId, GetDocumentQuery query,
        CancellationToken ct) => repository.GetAsync(actorId, query.DocumentId, ct);
}
