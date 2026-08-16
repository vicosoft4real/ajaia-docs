using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Sharing.ListShares;

public sealed class ListDocumentSharesHandler(IDocumentShareRepository shares)
{
    public Task<Result<IReadOnlyList<DocumentShareDto>>> HandleAsync(Guid actorId,
        Guid documentId, CancellationToken ct) => shares.ListAsync(actorId, documentId, ct);
}
