using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Sharing;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using Dapper;
using Npgsql;

namespace AjaiaDocs.Infrastructure.Data.Repositories;

public sealed class DocumentShareRepository(AjaiaDbConnectionFactory connections)
    : IDocumentShareRepository
{
    public async Task<Result<IReadOnlyList<DocumentShareDto>>> ListAsync(Guid actorId,
        Guid documentId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var authorization = await AuthorizeOwnerAsync(connection, actorId, documentId, ct);
        if (!authorization.IsSuccess)
        {
            return Result<IReadOnlyList<DocumentShareDto>>.Failure(authorization.Error);
        }

        var rows = await connection.QueryAsync<ShareRow>(new CommandDefinition(
            """
            SELECT share.document_id AS DocumentId,
                   share.user_id AS UserId,
                   collaborator.display_name AS DisplayName,
                   collaborator.email AS Email,
                   collaborator.avatar_color AS AvatarColor,
                   share.created_at AS CreatedAt
            FROM document_shares share
            JOIN app_users collaborator ON collaborator.id = share.user_id
            WHERE share.document_id = @DocumentId
            ORDER BY collaborator.display_name, collaborator.id;
            """, new { DocumentId = documentId }, cancellationToken: ct));

        return Result<IReadOnlyList<DocumentShareDto>>.Success(rows.Select(ToDto).ToArray());
    }

    public async Task<Result<DocumentShareDto>> GrantAsync(Guid actorId, Guid documentId,
        Guid targetUserId, DateTimeOffset now, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var authorization = await AuthorizeOwnerAsync(connection, actorId, documentId, ct);
        if (!authorization.IsSuccess)
        {
            return Result<DocumentShareDto>.Failure(authorization.Error);
        }

        var targetExists = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            """
            SELECT EXISTS (
                SELECT 1
                FROM app_users
                WHERE id = @TargetUserId
                  AND is_seeded = true);
            """, new { TargetUserId = targetUserId }, cancellationToken: ct));
        if (!targetExists)
        {
            return Result<DocumentShareDto>.Failure(UserNotFound());
        }

        try
        {
            var row = await connection.QuerySingleAsync<ShareRow>(new CommandDefinition(
                """
                WITH inserted AS (
                    INSERT INTO document_shares
                        (document_id, user_id, shared_by_user_id, created_at)
                    VALUES
                        (@DocumentId, @TargetUserId, @ActorId, @CreatedAt)
                    RETURNING document_id, user_id, created_at
                )
                SELECT inserted.document_id AS DocumentId,
                       inserted.user_id AS UserId,
                       collaborator.display_name AS DisplayName,
                       collaborator.email AS Email,
                       collaborator.avatar_color AS AvatarColor,
                       inserted.created_at AS CreatedAt
                FROM inserted
                JOIN app_users collaborator ON collaborator.id = inserted.user_id;
                """, new
                {
                    ActorId = actorId,
                    DocumentId = documentId,
                    TargetUserId = targetUserId,
                    CreatedAt = now.UtcDateTime
                }, cancellationToken: ct));

            return Result<DocumentShareDto>.Success(ToDto(row));
        }
        catch (PostgresException exception) when (exception.SqlState == "23505")
        {
            return Result<DocumentShareDto>.Failure(DuplicateShare());
        }
        catch (PostgresException exception) when (exception.SqlState == "23514")
        {
            return Result<DocumentShareDto>.Failure(SelfShare());
        }
    }

    public async Task<Result<bool>> RevokeAsync(Guid actorId, Guid documentId,
        Guid targetUserId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var authorization = await AuthorizeOwnerAsync(connection, actorId, documentId, ct);
        if (!authorization.IsSuccess)
        {
            return Result<bool>.Failure(authorization.Error);
        }

        var deleted = await connection.ExecuteAsync(new CommandDefinition(
            """
            DELETE FROM document_shares
            WHERE document_id = @DocumentId
              AND user_id = @TargetUserId;
            """, new { DocumentId = documentId, TargetUserId = targetUserId },
            cancellationToken: ct));

        return deleted == 1
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(ShareNotFound());
    }

    private static async Task<Result<bool>> AuthorizeOwnerAsync(NpgsqlConnection connection,
        Guid actorId, Guid documentId, CancellationToken ct)
    {
        var access = await connection.QuerySingleOrDefaultAsync<DocumentAccessRow>(
            new CommandDefinition(
                """
                SELECT document.owner_id AS OwnerId,
                       EXISTS (
                           SELECT 1
                           FROM document_shares actor_share
                           WHERE actor_share.document_id = document.id
                             AND actor_share.user_id = @ActorId) AS HasShare
                FROM documents document
                WHERE document.id = @DocumentId;
                """, new { ActorId = actorId, DocumentId = documentId }, cancellationToken: ct));
        if (access is null)
        {
            return Result<bool>.Failure(NotFound());
        }

        var decision = DocumentAccessPolicy.Decide(actorId, access.OwnerId, access.HasShare,
            DocumentOperation.Share);
        if (decision.Allowed)
        {
            return Result<bool>.Success(true);
        }

        return Result<bool>.Failure(decision.IsNotFound ? NotFound() : OwnerRequired());
    }

    private static DocumentShareDto ToDto(ShareRow row) => new(row.DocumentId, row.UserId,
        row.DisplayName, row.Email, row.AvatarColor,
        new DateTimeOffset(DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc)));

    private static AjaiaError NotFound() => new("not_found", "The document was not found.",
        ErrorType.NotFound);

    private static AjaiaError OwnerRequired() => new("owner_required",
        "Only the document owner can manage sharing.", ErrorType.Forbidden);

    private static AjaiaError UserNotFound() => new("user_not_found",
        "The selected user was not found.", ErrorType.Validation);

    private static AjaiaError DuplicateShare() => new("duplicate_share",
        "The user already has access to the document.", ErrorType.Conflict);

    private static AjaiaError SelfShare() => new("self_share",
        "The document owner already has access.", ErrorType.Validation);

    private static AjaiaError ShareNotFound() => new("share_not_found",
        "The document share was not found.", ErrorType.NotFound);

    private sealed class DocumentAccessRow
    {
        public Guid OwnerId { get; init; }
        public bool HasShare { get; init; }
    }

    private sealed class ShareRow
    {
        public Guid DocumentId { get; init; }
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string AvatarColor { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
