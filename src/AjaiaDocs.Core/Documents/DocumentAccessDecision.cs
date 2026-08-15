namespace AjaiaDocs.Core.Documents;

public sealed record DocumentAccessDecision(bool Allowed, bool IsNotFound, string? ErrorCode);
