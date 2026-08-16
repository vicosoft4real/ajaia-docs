using AjaiaDocs.Core.Documents;
using FluentValidation;

namespace AjaiaDocs.Application.Features.Documents.Rename;

public sealed class RenameDocumentValidator : AbstractValidator<RenameDocumentCommand>
{
    public RenameDocumentValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .WithErrorCode("title_required")
            .WithMessage("A title is required.")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithErrorCode("title_required")
            .WithMessage("A title is required.")
            .Must(title => title is null || title.Trim().Length <= Document.MaxTitleLength)
            .WithErrorCode("title_too_long")
            .WithMessage($"A title cannot exceed {Document.MaxTitleLength} characters.");
    }
}
