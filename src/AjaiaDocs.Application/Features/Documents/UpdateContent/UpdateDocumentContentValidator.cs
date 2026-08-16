using System.Text;
using System.Text.Json;
using AjaiaDocs.Core.Documents;
using FluentValidation;

namespace AjaiaDocs.Application.Features.Documents.UpdateContent;

public sealed class UpdateDocumentContentValidator : AbstractValidator<UpdateDocumentContentCommand>
{
    public UpdateDocumentContentValidator()
    {
        RuleFor(command => command.ContentFormat)
            .Must(IsSupportedFormat)
            .WithErrorCode("invalid_content_format")
            .WithMessage("The content format is invalid.");

        RuleFor(command => command.Content)
            .Must(content => content is not null &&
                Encoding.UTF8.GetByteCount(content) <= Document.MaxContentBytes)
            .WithErrorCode("content_too_large")
            .WithMessage($"Content cannot exceed {Document.MaxContentBytes} bytes.");

        RuleFor(command => command.Content)
            .Must((command, content) => command.ContentFormat != "lexical" ||
                IsValidLexicalState(content))
            .WithErrorCode("invalid_editor_state")
            .WithMessage("The Lexical editor state is invalid.");
    }

    private static bool IsSupportedFormat(string? contentFormat) => contentFormat is
        "lexical" or "markdown" or "plainText";

    private static bool IsValidLexicalState(string? content)
    {
        if (content is null)
        {
            return false;
        }

        try
        {
            using var state = JsonDocument.Parse(content);
            if (!state.RootElement.TryGetProperty("root", out var root) ||
                root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            return root.TryGetProperty("type", out var type) &&
                   type.ValueKind == JsonValueKind.String &&
                   type.GetString() == "root" &&
                   root.TryGetProperty("version", out var version) &&
                   version.TryGetInt32(out var versionNumber) &&
                   versionNumber > 0 &&
                   root.TryGetProperty("children", out var children) &&
                   children.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
