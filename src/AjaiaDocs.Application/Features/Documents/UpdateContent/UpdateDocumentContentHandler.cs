using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;
using FluentValidation;

namespace AjaiaDocs.Application.Features.Documents.UpdateContent;

public sealed class UpdateDocumentContentHandler(
    IDocumentRepository repository,
    IValidator<UpdateDocumentContentCommand> validator)
{
    public async Task<Result<DocumentDto>> HandleAsync(Guid actorId, Guid documentId,
        UpdateDocumentContentCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var failure = validation.Errors[0];
            return Result<DocumentDto>.Failure(new AjaiaError(failure.ErrorCode,
                failure.ErrorMessage, ErrorType.Validation));
        }

        var format = command.ContentFormat switch
        {
            "lexical" => ContentFormat.Lexical,
            "markdown" => ContentFormat.Markdown,
            "plainText" => ContentFormat.PlainText,
            _ => throw new InvalidOperationException("Validated content format was not supported.")
        };

        return await repository.UpdateContentAsync(actorId, documentId, command.Content,
            command.PlainText, format, command.ExpectedVersion, ct);
    }
}
