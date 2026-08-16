namespace AjaiaDocs.Api.Common;

public sealed record ProblemResponse(string Code, string Detail,
    IReadOnlyDictionary<string, string[]>? Errors = null);
