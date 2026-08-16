using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Sharing.GetShareCandidates;

public sealed class GetShareCandidatesHandler(IUserRepository users)
{
    public Task<Result<IReadOnlyList<ShareCandidateDto>>> HandleAsync(Guid actorId,
        Guid documentId, CancellationToken ct) =>
        users.ListShareCandidatesAsync(actorId, documentId, ct);
}
