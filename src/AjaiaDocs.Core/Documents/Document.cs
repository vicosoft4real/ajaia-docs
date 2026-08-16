using System.Text;
using AjaiaDocs.Core.Common;

namespace AjaiaDocs.Core.Documents;

public sealed record Document(
    Guid Id,
    Guid OwnerId,
    string Title,
    ContentFormat ContentFormat,
    string Content,
    string PlainText,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public const int MaxTitleLength = 120;
    public const int MaxContentBytes = 2 * 1024 * 1024;

    public static Result<Document> Create(Guid id, Guid ownerId, string? title,
        ContentFormat contentFormat, string content, string plainText, DateTimeOffset now)
    {
        var titleResult = ValidateTitle(title);
        if (!titleResult.IsSuccess)
        {
            return Result<Document>.Failure(titleResult.Error);
        }

        var contentResult = ValidateContent(content, contentFormat);
        if (!contentResult.IsSuccess)
        {
            return Result<Document>.Failure(contentResult.Error);
        }

        return Result<Document>.Success(new Document(id, ownerId, titleResult.Value, contentFormat,
            content, plainText, 1, now, now));
    }

    public Result<Document> Rename(string? title, int expectedVersion, DateTimeOffset now)
    {
        if (expectedVersion != Version)
        {
            return Result<Document>.Failure(Conflict());
        }

        var titleResult = ValidateTitle(title);
        if (!titleResult.IsSuccess)
        {
            return Result<Document>.Failure(titleResult.Error);
        }

        return Result<Document>.Success(this with
        {
            Title = titleResult.Value,
            Version = Version + 1,
            UpdatedAt = now
        });
    }

    public Result<Document> UpdateContent(string content, string plainText, ContentFormat contentFormat,
        int expectedVersion, DateTimeOffset now)
    {
        if (expectedVersion != Version)
        {
            return Result<Document>.Failure(Conflict());
        }

        var contentResult = ValidateContent(content, contentFormat);
        if (!contentResult.IsSuccess)
        {
            return Result<Document>.Failure(contentResult.Error);
        }

        return Result<Document>.Success(this with
        {
            Content = content,
            PlainText = plainText,
            ContentFormat = contentFormat,
            Version = Version + 1,
            UpdatedAt = now
        });
    }

    private static Result<string> ValidateTitle(string? title)
    {
        var trimmedTitle = title?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            return Result<string>.Failure(new AjaiaError("title_required", "A title is required.",
                ErrorType.Validation));
        }

        if (trimmedTitle.Length > MaxTitleLength)
        {
            return Result<string>.Failure(new AjaiaError("title_too_long",
                $"A title cannot exceed {MaxTitleLength} characters.", ErrorType.Validation));
        }

        return Result<string>.Success(trimmedTitle);
    }

    private static Result<bool> ValidateContent(string content, ContentFormat contentFormat)
    {
        if (!Enum.IsDefined(contentFormat))
        {
            return Result<bool>.Failure(new AjaiaError("invalid_content_format",
                "The content format is invalid.", ErrorType.Validation));
        }

        if (Encoding.UTF8.GetByteCount(content) > MaxContentBytes)
        {
            return Result<bool>.Failure(new AjaiaError("content_too_large",
                $"Content cannot exceed {MaxContentBytes} bytes.", ErrorType.Validation));
        }

        return Result<bool>.Success(true);
    }

    private static AjaiaError Conflict() => new("conflict", "The document has changed.",
        ErrorType.Conflict);
}
