using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.UnitTests.Fixtures;

public static class DocumentFixtures
{
    public static DocumentDto Dto(Document document, bool isOwner) => new(
        document.Id, document.OwnerId, document.Title,
        document.ContentFormat switch
        {
            ContentFormat.Lexical => "lexical",
            ContentFormat.Markdown => "markdown",
            ContentFormat.PlainText => "plainText",
            _ => throw new ArgumentOutOfRangeException()
        },
        document.Content, document.PlainText, document.Version, document.CreatedAt,
        document.UpdatedAt,
        new UserSummaryDto(document.OwnerId, "Amina Okafor",
            "amina@example.test", "#365CF5"),
        isOwner, true, isOwner, isOwner, isOwner);
}
