using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using FluentValidation;

namespace AjaiaDocs.Application.Features.Documents.CreateDocument;

public sealed class CreateDocumentHandler(
    IDocumentRepository repository,
    IValidator<CreateDocumentCommand> validator,
    TimeProvider timeProvider)
{
    public async Task<Result<DocumentDto>> HandleAsync(Guid actorId,
        CreateDocumentCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var failure = validation.Errors[0];
            return Result<DocumentDto>.Failure(new AjaiaError(failure.ErrorCode,
                failure.ErrorMessage, ErrorType.Validation));
        }

        var document = Document.Create(Guid.CreateVersion7(), actorId,
            command.Title ?? "Untitled document", ContentFormat.Lexical,
            DocumentContentDefaults.EmptyLexical, string.Empty, timeProvider.GetUtcNow());
        if (!document.IsSuccess)
        {
            return Result<DocumentDto>.Failure(document.Error);
        }

        return await repository.CreateAsync(document.Value, ct);
    }
}
