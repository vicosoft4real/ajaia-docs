using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Sharing.GrantShare;

public sealed class GrantDocumentShareHandler(
    IDocumentShareRepository shares,
    TimeProvider timeProvider)
{
    public Task<Result<DocumentShareDto>> HandleAsync(Guid actorId, Guid documentId,
        Guid targetUserId, CancellationToken ct) =>
        shares.GrantAsync(actorId, documentId, targetUserId, timeProvider.GetUtcNow(), ct);
}
