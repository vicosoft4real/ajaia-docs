namespace AjaiaDocs.Application.Features.Sharing;

public sealed record ShareCandidateDto(Guid Id, string DisplayName, string Email,
    string AvatarColor);

public sealed record DocumentShareDto(Guid DocumentId, Guid UserId,
    string DisplayName, string Email, string AvatarColor,
    DateTimeOffset CreatedAt);
