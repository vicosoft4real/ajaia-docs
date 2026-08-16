using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.UnitTests.Core;

public sealed class DocumentAccessPolicyTests
{
    [Theory]
    [InlineData(DocumentOperation.Read, true)]
    [InlineData(DocumentOperation.EditContent, true)]
    [InlineData(DocumentOperation.Rename, false)]
    [InlineData(DocumentOperation.Share, false)]
    [InlineData(DocumentOperation.RevokeShare, false)]
    [InlineData(DocumentOperation.Delete, false)]
    public void Collaborator_only_reads_and_edits(DocumentOperation operation, bool allowed)
    {
        var decision = DocumentAccessPolicy.Decide(Guid.NewGuid(), Guid.NewGuid(), true, operation);

        Assert.Equal(allowed, decision.Allowed);
        Assert.Equal(allowed ? null : "owner_required", decision.ErrorCode);
    }

    [Fact]
    public void No_access_is_reported_as_not_found()
    {
        var decision = DocumentAccessPolicy.Decide(Guid.NewGuid(), Guid.NewGuid(), false,
            DocumentOperation.Read);

        Assert.False(decision.Allowed);
        Assert.True(decision.IsNotFound);
    }

    [Theory]
    [InlineData(DocumentOperation.Read)]
    [InlineData(DocumentOperation.EditContent)]
    [InlineData(DocumentOperation.Rename)]
    [InlineData(DocumentOperation.Share)]
    [InlineData(DocumentOperation.RevokeShare)]
    [InlineData(DocumentOperation.Delete)]
    public void Owner_can_perform_every_operation(DocumentOperation operation)
    {
        var ownerId = Guid.NewGuid();

        var decision = DocumentAccessPolicy.Decide(ownerId, ownerId, false, operation);

        Assert.True(decision.Allowed);
        Assert.False(decision.IsNotFound);
        Assert.Null(decision.ErrorCode);
    }
}
