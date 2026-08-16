using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Core.Documents;

public sealed record DocumentShare(Guid DocumentId, Guid UserId, Guid SharedByUserId,
    DateTimeOffset CreatedAt)
{
    public static Result<DocumentShare> Create(Guid documentId, Guid ownerId, Guid userId,
        Guid sharedByUserId, DateTimeOffset now)
    {
        if (userId == ownerId)
        {
            return Result<DocumentShare>.Failure(new AjaiaError("owner_cannot_be_collaborator",
                "The owner already has access to the document.", ErrorType.Validation));
        }

        return Result<DocumentShare>.Success(new DocumentShare(documentId, userId, sharedByUserId, now));
    }
}
