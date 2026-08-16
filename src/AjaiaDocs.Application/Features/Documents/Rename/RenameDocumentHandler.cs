using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;
using FluentValidation;

namespace AjaiaDocs.Application.Features.Documents.Rename;

public sealed class RenameDocumentHandler(
    IDocumentRepository repository,
    IValidator<RenameDocumentCommand> validator)
{
    public async Task<Result<DocumentDto>> HandleAsync(Guid actorId, Guid documentId,
        RenameDocumentCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var failure = validation.Errors[0];
            return Result<DocumentDto>.Failure(new AjaiaError(failure.ErrorCode,
                failure.ErrorMessage, ErrorType.Validation));
        }

        return await repository.RenameAsync(actorId, documentId, command.Title.Trim(),
            command.ExpectedVersion, ct);
    }
}
