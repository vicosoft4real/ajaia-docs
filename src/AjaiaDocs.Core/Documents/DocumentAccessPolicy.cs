namespace AjaiaDocs.Core.Documents;

public static class DocumentAccessPolicy
{
    public static DocumentAccessDecision Decide(Guid actorId, Guid ownerId, bool hasShare,
        DocumentOperation operation)
    {
        if (actorId == ownerId)
        {
            return new DocumentAccessDecision(true, false, null);
        }

        if (hasShare && operation is DocumentOperation.Read or DocumentOperation.EditContent)
        {
            return new DocumentAccessDecision(true, false, null);
        }

        if (!hasShare)
        {
            return new DocumentAccessDecision(false, true, "not_found");
        }

        return new DocumentAccessDecision(false, false, "owner_required");
    }
}
