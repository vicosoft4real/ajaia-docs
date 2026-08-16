using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Sharing;
using AjaiaDocs.Application.Features.Sharing.GetShareCandidates;
using AjaiaDocs.Application.Features.Sharing.GrantShare;
using AjaiaDocs.Application.Features.Sharing.ListShares;
using AjaiaDocs.Application.Features.Sharing.RevokeShare;
using AjaiaDocs.Core.Common;
using NSubstitute;

namespace AjaiaDocs.UnitTests.Application;

public sealed class SharingHandlerTests
{
    [Fact]
    public async Task Candidates_are_forwarded_with_avatar_metadata()
    {
        var actorId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        ShareCandidateDto[] candidates =
        [
            new(Guid.NewGuid(), "Chidi Okeke", "chidi@example.test", "#25A77A")
        ];
        var users = Substitute.For<IUserRepository>();
        users.ListShareCandidatesAsync(actorId, documentId, CancellationToken.None)
            .Returns(Result<IReadOnlyList<ShareCandidateDto>>.Success(candidates));
        var handler = new GetShareCandidatesHandler(users);

        var result = await handler.HandleAsync(actorId, documentId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(candidates, result.Value);
        Assert.Equal("#25A77A", result.Value[0].AvatarColor);
    }

    [Fact]
    public async Task List_preserves_owner_required_from_the_repository()
    {
        var forbidden = new AjaiaError("owner_required", "Owner required.",
            ErrorType.Forbidden);
        var repository = Substitute.For<IDocumentShareRepository>();
        repository.ListAsync(Arg.Any<Guid>(), Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<DocumentShareDto>>.Failure(forbidden));
        var handler = new ListDocumentSharesHandler(repository);

        var result = await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Same(forbidden, result.Error);
    }

    [Fact]
    public async Task Grant_forwards_actor_target_and_time()
    {
        var actorId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 8, 16, 9, 30, 0, TimeSpan.Zero);
        var expected = new DocumentShareDto(documentId, targetId, "Tayo Bello",
            "tayo@example.test", "#C77A15", now);
        var repository = Substitute.For<IDocumentShareRepository>();
        repository.GrantAsync(actorId, documentId, targetId, now,
                CancellationToken.None)
            .Returns(Result<DocumentShareDto>.Success(expected));
        var timeProvider = new FixedTimeProvider(now);
        var handler = new GrantDocumentShareHandler(repository, timeProvider);

        var result = await handler.HandleAsync(actorId, documentId, targetId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(expected, result.Value);
        await repository.Received(1).GrantAsync(actorId, documentId, targetId, now,
            CancellationToken.None);
    }

    [Fact]
    public async Task Revoke_forwards_the_repository_result()
    {
        var actorId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var repository = Substitute.For<IDocumentShareRepository>();
        repository.RevokeAsync(actorId, documentId, targetId, CancellationToken.None)
            .Returns(Result<bool>.Success(true));
        var handler = new RevokeDocumentShareHandler(repository);

        var result = await handler.HandleAsync(actorId, documentId, targetId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        await repository.Received(1).RevokeAsync(actorId, documentId, targetId,
            CancellationToken.None);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
