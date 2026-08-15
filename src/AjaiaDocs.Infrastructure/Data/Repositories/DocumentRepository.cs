using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using Dapper;

namespace AjaiaDocs.Infrastructure.Data.Repositories;

public sealed class DocumentRepository(AjaiaDbConnectionFactory connections) : IDocumentRepository
{
    private const string Projection =
        """
        SELECT d.id AS Id,
               d.owner_id AS OwnerId,
               d.title AS Title,
               d.content_format AS ContentFormat,
               d.content AS Content,
               d.plain_text AS PlainText,
               d.version AS Version,
               d.created_at AS CreatedAt,
               d.updated_at AS UpdatedAt,
               owner.display_name AS OwnerDisplayName,
               owner.email AS OwnerEmail,
               owner.avatar_color AS OwnerAvatarColor,
               d.owner_id = @ActorId AS IsOwner
        FROM documents d
        JOIN app_users owner ON owner.id = d.owner_id
        """;

    private const string AccessiblePredicate =
        """
        (d.owner_id = @ActorId
         OR EXISTS (
             SELECT 1
             FROM document_shares access_share
             WHERE access_share.document_id = d.id
               AND access_share.user_id = @ActorId))
        """;

    public async Task<Result<DocumentDto>> CreateAsync(Document document, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleAsync<DocumentRow>(new CommandDefinition(
            """
            WITH inserted AS (
                INSERT INTO documents
                    (id, owner_id, title, content_format, content, plain_text, version, created_at, updated_at)
                VALUES
                    (@Id, @OwnerId, @Title, @ContentFormat, @Content, @PlainText, @Version, @CreatedAt, @UpdatedAt)
                RETURNING *
            )
            SELECT d.id AS Id,
                   d.owner_id AS OwnerId,
                   d.title AS Title,
                   d.content_format AS ContentFormat,
                   d.content AS Content,
                   d.plain_text AS PlainText,
                   d.version AS Version,
                   d.created_at AS CreatedAt,
                   d.updated_at AS UpdatedAt,
                   owner.display_name AS OwnerDisplayName,
                   owner.email AS OwnerEmail,
                   owner.avatar_color AS OwnerAvatarColor,
                   true AS IsOwner
            FROM inserted d
            JOIN app_users owner ON owner.id = d.owner_id;
            """,
            new
            {
                document.Id,
                document.OwnerId,
                document.Title,
                ContentFormat = ToStorageValue(document.ContentFormat),
                document.Content,
                document.PlainText,
                document.Version,
                CreatedAt = document.CreatedAt.UtcDateTime,
                UpdatedAt = document.UpdatedAt.UtcDateTime
            }, cancellationToken: ct));

        return Result<DocumentDto>.Success(ToDocument(row));
    }

    public async Task<Result<IReadOnlyList<DocumentListItemDto>>> ListAsync(Guid actorId,
        DocumentScope scope, CancellationToken ct)
    {
        const string scopePredicate =
            """
            AND (@Scope = 'All'
                 OR (@Scope = 'Owned' AND d.owner_id = @ActorId)
                 OR (@Scope = 'Shared'
                     AND d.owner_id <> @ActorId
                     AND EXISTS (
                         SELECT 1
                         FROM document_shares scope_share
                         WHERE scope_share.document_id = d.id
                           AND scope_share.user_id = @ActorId)))
            ORDER BY d.updated_at DESC, d.id
            """;

        await using var connection = await connections.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<DocumentRow>(new CommandDefinition(
            $"{Projection} WHERE {AccessiblePredicate} {scopePredicate}",
            new { ActorId = actorId, Scope = scope.ToString() }, cancellationToken: ct));

        return Result<IReadOnlyList<DocumentListItemDto>>.Success(rows.Select(ToListItem).ToArray());
    }

    public async Task<Result<DocumentDto>> GetAsync(Guid actorId, Guid documentId,
        CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<DocumentRow>(new CommandDefinition(
            $"{Projection} WHERE d.id = @DocumentId AND {AccessiblePredicate}",
            new { ActorId = actorId, DocumentId = documentId }, cancellationToken: ct));

        return row is null
            ? Result<DocumentDto>.Failure(NotFound())
            : Result<DocumentDto>.Success(ToDocument(row));
    }

    public Task<Result<DocumentDto>> UpdateContentAsync(Guid actorId, Guid documentId,
        string content, string plainText, ContentFormat format, int expectedVersion,
        CancellationToken ct) => throw new NotSupportedException("Content updates are implemented in Task 4.");

    public Task<Result<DocumentDto>> RenameAsync(Guid actorId, Guid documentId, string title,
        int expectedVersion, CancellationToken ct) =>
        throw new NotSupportedException("Renames are implemented in Task 4.");

    public Task<Result<bool>> DeleteAsync(Guid actorId, Guid documentId, CancellationToken ct) =>
        throw new NotSupportedException("Deletion is implemented in Task 4.");

    private static DocumentListItemDto ToListItem(DocumentRow row) => new(row.Id, row.OwnerId,
        row.Title, row.ContentFormat, row.PlainText, row.Version, AsOffset(row.UpdatedAt),
        ToOwner(row), row.IsOwner);

    private static DocumentDto ToDocument(DocumentRow row) => new(row.Id, row.OwnerId, row.Title,
        row.ContentFormat, row.Content, row.PlainText, row.Version, AsOffset(row.CreatedAt),
        AsOffset(row.UpdatedAt), ToOwner(row), row.IsOwner, true, row.IsOwner, row.IsOwner,
        row.IsOwner);

    private static UserSummaryDto ToOwner(DocumentRow row) => new(row.OwnerId,
        row.OwnerDisplayName, row.OwnerEmail, row.OwnerAvatarColor);

    private static DateTimeOffset AsOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static string ToStorageValue(ContentFormat format) => format switch
    {
        ContentFormat.Lexical => "lexical",
        ContentFormat.Markdown => "markdown",
        ContentFormat.PlainText => "plainText",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown content format.")
    };

    private static AjaiaError NotFound() => new("not_found", "The document was not found.",
        ErrorType.NotFound);

    private sealed class DocumentRow
    {
        public Guid Id { get; init; }
        public Guid OwnerId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string ContentFormat { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string PlainText { get; init; } = string.Empty;
        public int Version { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string OwnerDisplayName { get; init; } = string.Empty;
        public string OwnerEmail { get; init; } = string.Empty;
        public string OwnerAvatarColor { get; init; } = string.Empty;
        public bool IsOwner { get; init; }
    }
}
