using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.Application.Features.Import;

public sealed class ImportDocumentHandler(
    StrictTextImportParser parser,
    IDocumentRepository documents,
    TimeProvider timeProvider)
{
    public async Task<Result<DocumentDto>> HandleAsync(Guid actorId, string fileName,
        ReadOnlyMemory<byte> bytes, CancellationToken ct)
    {
        var imported = parser.ParseFile(fileName, bytes.Span);
        if (!imported.IsSuccess)
        {
            return Result<DocumentDto>.Failure(imported.Error);
        }

        var source = imported.Value;
        var document = Document.Create(Guid.CreateVersion7(), actorId, source.Title,
            source.Format, source.Content, source.PlainText, timeProvider.GetUtcNow());
        if (!document.IsSuccess)
        {
            return Result<DocumentDto>.Failure(document.Error);
        }

        return await documents.CreateAsync(document.Value, ct);
    }
}
