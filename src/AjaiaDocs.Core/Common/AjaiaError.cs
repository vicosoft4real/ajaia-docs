namespace AjaiaDocs.Core.Common;

public sealed record AjaiaError(string Code, string Message, ErrorType Type);
