using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Documents.ListDocuments;

public sealed class ListDocumentsHandler(IDocumentRepository repository)
{
    public Task<Result<IReadOnlyList<DocumentListItemDto>>> HandleAsync(Guid actorId,
        ListDocumentsQuery query, CancellationToken ct) =>
        repository.ListAsync(actorId, query.Scope, ct);
}
