namespace AjaiaDocs.Application.Features.Documents;

public sealed record UserSummaryDto(Guid Id, string DisplayName, string Email,
    string AvatarColor);

public sealed record DocumentListItemDto(Guid Id, Guid OwnerId, string Title,
    string ContentFormat, string PlainText, int Version, DateTimeOffset UpdatedAt,
    UserSummaryDto Owner, bool IsOwner);

public sealed record DocumentDto(Guid Id, Guid OwnerId, string Title,
    string ContentFormat, string Content, string PlainText, int Version,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, UserSummaryDto Owner,
    bool IsOwner, bool CanEdit, bool CanRename, bool CanShare, bool CanDelete);
