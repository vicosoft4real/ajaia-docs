using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Users;
using Dapper;

namespace AjaiaDocs.Infrastructure.Data.Repositories;

public sealed class UserRepository(AjaiaDbConnectionFactory connections) : IUserRepository
{
    public async Task<Result<User>> GetSeededAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(
            """
            SELECT id AS Id, email AS Email, display_name AS DisplayName, created_at AS CreatedAt
            FROM app_users
            WHERE id = @UserId AND is_seeded = true
            """, new { UserId = userId }, cancellationToken: ct));

        return row is null
            ? Result<User>.Failure(new AjaiaError("not_found", "The user was not found.",
                ErrorType.NotFound))
            : Result<User>.Success(ToUser(row));
    }

    public async Task<Result<IReadOnlyList<User>>> ListShareCandidatesAsync(Guid actorId,
        Guid documentId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<UserRow>(new CommandDefinition(
            """
            SELECT candidate.id AS Id,
                   candidate.email AS Email,
                   candidate.display_name AS DisplayName,
                   candidate.created_at AS CreatedAt
            FROM app_users candidate
            WHERE candidate.is_seeded = true
              AND candidate.id <> @ActorId
              AND NOT EXISTS (
                  SELECT 1
                  FROM document_shares existing_share
                  WHERE existing_share.document_id = @DocumentId
                    AND existing_share.user_id = candidate.id)
            ORDER BY candidate.display_name, candidate.id
            """, new { ActorId = actorId, DocumentId = documentId }, cancellationToken: ct));

        return Result<IReadOnlyList<User>>.Success(rows.Select(ToUser).ToArray());
    }

    private static User ToUser(UserRow row) => new(row.Id, row.Email, row.DisplayName,
        new DateTimeOffset(DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc)));

    private sealed class UserRow
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
