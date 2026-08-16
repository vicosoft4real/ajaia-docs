using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.Application.Features.Import;

public sealed record ImportedText(string Title, ContentFormat Format,
    string Content, string PlainText);
