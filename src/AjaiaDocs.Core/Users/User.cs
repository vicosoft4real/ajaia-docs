namespace AjaiaDocs.Core.Users;

public sealed record User(Guid Id, string Email, string DisplayName, DateTimeOffset CreatedAt);
