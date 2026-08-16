namespace AjaiaDocs.Api.Contracts;

public sealed record StartSessionRequest(Guid UserId);

public sealed record CreateDocumentRequest(string? Title);

public sealed record UpdateDocumentContentRequest(string ContentFormat,
    string Content, string PlainText, int ExpectedVersion);

public sealed record RenameDocumentRequest(string Title, int ExpectedVersion);

public sealed record GrantShareRequest(Guid UserId);

public sealed record SessionUserResponse(Guid Id, string DisplayName, string Email,
    string AvatarColor);
