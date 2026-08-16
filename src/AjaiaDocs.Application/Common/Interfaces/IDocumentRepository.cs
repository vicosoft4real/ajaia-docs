using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.Application.Common.Interfaces;

public interface IDocumentRepository
{
    Task<Result<DocumentDto>> CreateAsync(Document document, CancellationToken ct);

    Task<Result<IReadOnlyList<DocumentListItemDto>>> ListAsync(Guid actorId,
        DocumentScope scope, CancellationToken ct);

    Task<Result<DocumentDto>> GetAsync(Guid actorId, Guid documentId,
        CancellationToken ct);

    Task<Result<DocumentDto>> UpdateContentAsync(Guid actorId, Guid documentId,
        string content, string plainText, ContentFormat format, int expectedVersion,
        CancellationToken ct);

    Task<Result<DocumentDto>> RenameAsync(Guid actorId, Guid documentId,
        string title, int expectedVersion, CancellationToken ct);

    Task<Result<bool>> DeleteAsync(Guid actorId, Guid documentId, CancellationToken ct);
}
