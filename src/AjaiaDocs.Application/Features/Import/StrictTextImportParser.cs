using System.Text;
using AjaiaDocs.Core.Common;
using AjaiaDocs.Core.Documents;

namespace AjaiaDocs.Application.Features.Import;

public sealed class StrictTextImportParser
{
    public const int MaxFileBytes = 1024 * 1024;

    public static Result<ImportedText> Parse(string fileName, ReadOnlySpan<byte> bytes)
    {
        var normalizedFileName = fileName?.Trim() ?? string.Empty;
        var extension = Path.GetExtension(normalizedFileName);
        var format = extension.ToLowerInvariant() switch
        {
            ".txt" => ContentFormat.PlainText,
            ".md" => ContentFormat.Markdown,
            _ => (ContentFormat?)null
        };
        if (format is null)
        {
            return Result<ImportedText>.Failure(new AjaiaError("unsupported_file_type",
                "Only .txt and .md files are supported.", ErrorType.Validation));
        }

        if (bytes.Length > MaxFileBytes)
        {
            return Result<ImportedText>.Failure(new AjaiaError("file_too_large",
                $"The file cannot exceed {MaxFileBytes} bytes.", ErrorType.Validation));
        }

        string content;
        try
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);
            content = utf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Result<ImportedText>.Failure(new AjaiaError("invalid_utf8",
                "The file must contain valid UTF-8 text.", ErrorType.Validation));
        }

        var title = Path.GetFileNameWithoutExtension(normalizedFileName).Trim();
        if (title.Length > Document.MaxTitleLength)
        {
            title = title[..Document.MaxTitleLength].Trim();
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Untitled document";
        }

        return Result<ImportedText>.Success(new ImportedText(title, format.Value,
            content, content));
    }

    public Result<ImportedText> ParseFile(string fileName, ReadOnlySpan<byte> bytes) =>
        Parse(fileName, bytes);
}
