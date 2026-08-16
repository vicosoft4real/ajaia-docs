using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Application.Features.Documents.Delete;

public sealed class DeleteDocumentHandler(IDocumentRepository repository)
{
    public Task<Result<bool>> HandleAsync(Guid actorId, DeleteDocumentCommand command,
        CancellationToken ct) => repository.DeleteAsync(actorId, command.DocumentId, ct);
}
