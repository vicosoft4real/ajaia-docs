using AjaiaDocs.Core.Documents;
using FluentValidation;

namespace AjaiaDocs.Application.Features.Documents.CreateDocument;

public sealed class CreateDocumentValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentValidator()
    {
        RuleFor(command => command.Title)
            .Must(title => title is null || title.Trim().Length <= Document.MaxTitleLength)
            .WithErrorCode("title_too_long")
            .WithMessage($"A title cannot exceed {Document.MaxTitleLength} characters.");
    }
}
