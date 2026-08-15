using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.UnitTests.Core;

public sealed class DocumentShareTests
{
    private static readonly Guid DocumentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_rejects_sharing_with_the_owner()
    {
        var result = DocumentShare.Create(DocumentId, OwnerId, OwnerId, OwnerId, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal("owner_cannot_be_collaborator", result.Error.Code);
    }

    [Fact]
    public void Create_builds_a_share_for_a_collaborator()
    {
        var collaboratorId = Guid.NewGuid();

        var result = DocumentShare.Create(DocumentId, OwnerId, collaboratorId, OwnerId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentId, result.Value.DocumentId);
        Assert.Equal(collaboratorId, result.Value.UserId);
        Assert.Equal(OwnerId, result.Value.SharedByUserId);
        Assert.Equal(Now, result.Value.CreatedAt);
    }
}
