namespace AjaiaDocs.Application.Features.Documents.UpdateContent;

public sealed record UpdateDocumentContentCommand(string ContentFormat,
    string Content, string PlainText, int ExpectedVersion);
