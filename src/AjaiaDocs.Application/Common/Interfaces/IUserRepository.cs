using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Users;

namespace AjaiaDocs.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<Result<User>> GetSeededAsync(Guid userId, CancellationToken ct);

    Task<Result<IReadOnlyList<User>>> ListShareCandidatesAsync(Guid actorId,
        Guid documentId, CancellationToken ct);
}
