using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Sharing.RevokeShare;

public sealed class RevokeDocumentShareHandler(IDocumentShareRepository shares)
{
    public Task<Result<bool>> HandleAsync(Guid actorId, Guid documentId,
        Guid targetUserId, CancellationToken ct) =>
        shares.RevokeAsync(actorId, documentId, targetUserId, ct);
}
