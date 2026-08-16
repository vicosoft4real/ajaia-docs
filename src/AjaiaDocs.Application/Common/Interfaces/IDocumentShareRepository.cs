using AjaiaDocs.Application.Features.Sharing;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Common.Interfaces;

public interface IDocumentShareRepository
{
    Task<Result<IReadOnlyList<DocumentShareDto>>> ListAsync(Guid actorId,
        Guid documentId, CancellationToken ct);

    Task<Result<DocumentShareDto>> GrantAsync(Guid actorId, Guid documentId,
        Guid targetUserId, DateTimeOffset now, CancellationToken ct);

    Task<Result<bool>> RevokeAsync(Guid actorId, Guid documentId,
        Guid targetUserId, CancellationToken ct);
}
