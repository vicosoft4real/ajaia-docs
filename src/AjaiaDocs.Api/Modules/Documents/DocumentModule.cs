using AjaiaDocs.Api.Common;
using AjaiaDocs.Api.Contracts;
using AjaiaDocs.Api.Security;
using AjaiaDocs.Application.Common;
using AjaiaDocs.Application.Features.Documents.CreateDocument;
using AjaiaDocs.Application.Features.Documents.Delete;
using AjaiaDocs.Application.Features.Documents.GetDocument;
using AjaiaDocs.Application.Features.Documents.ListDocuments;
using AjaiaDocs.Application.Features.Documents.Rename;
using AjaiaDocs.Application.Features.Documents.UpdateContent;
using AjaiaDocs.Application.Features.Import;
using AjaiaDocs.Core.Common;
using Carter;
using Microsoft.AspNetCore.Http.Features;

namespace AjaiaDocs.Api.Modules.Documents;

public sealed class DocumentModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var documents = app.MapGroup("/api/documents").RequireAuthorization();

        documents.MapGet("", ListAsync);
        documents.MapPost("", CreateAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        documents.MapPost("/import", ImportAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        documents.MapGet("/{id:guid}", GetAsync);
        documents.MapPut("/{id:guid}/content", UpdateContentAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        documents.MapPut("/{id:guid}/title", RenameAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        documents.MapDelete("/{id:guid}", DeleteAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
    }

    private static async Task<IResult> ListAsync(string? scope, CurrentActor actor,
        ListDocumentsHandler handler, ResultHttpMapper mapper, CancellationToken ct)
    {
        if (!TryParseScope(scope, out var parsedScope))
        {
            return ResultHttpMapper.Problem("invalid_scope",
                "Scope must be one of: all, owned, or shared.",
                StatusCodes.Status400BadRequest,
                new Dictionary<string, string[]> { ["scope"] = ["The scope is invalid."] });
        }

        return mapper.ToHttpResult(await handler.HandleAsync(actor.UserId,
            new ListDocumentsQuery(parsedScope), ct));
    }

    private static async Task<IResult> CreateAsync(CreateDocumentRequest request,
        CurrentActor actor, CreateDocumentHandler handler, ResultHttpMapper mapper,
        CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId,
            new CreateDocumentCommand(request.Title), ct), StatusCodes.Status201Created);

    private static async Task<IResult> ImportAsync(HttpRequest request,
        CurrentActor actor, ImportDocumentHandler handler, ResultHttpMapper mapper,
        CancellationToken ct)
    {
        IFormCollection form;
        try
        {
            var formFeature = request.HttpContext.Features.Get<IFormFeature>();
            form = formFeature is null
                ? await request.ReadFormAsync(ct)
                : await formFeature.ReadFormAsync(ct);
        }
        catch (InvalidDataException)
        {
            return FileTooLarge();
        }

        var file = form.Files.GetFile("file");
        if (file is null)
        {
            return ResultHttpMapper.Problem("file_required", "A file is required.",
                StatusCodes.Status400BadRequest,
                new Dictionary<string, string[]> { ["file"] = ["A file is required."] });
        }

        if (file.Length > StrictTextImportParser.MaxFileBytes)
        {
            return FileTooLarge();
        }

        await using var source = file.OpenReadStream();
        using var destination = new MemoryStream((int)file.Length);
        var buffer = new byte[81920];
        while (true)
        {
            var read = await source.ReadAsync(buffer, ct);
            if (read == 0)
            {
                break;
            }

            if (destination.Length + read > StrictTextImportParser.MaxFileBytes)
            {
                return FileTooLarge();
            }

            await destination.WriteAsync(buffer.AsMemory(0, read), ct);
        }

        var result = await handler.HandleAsync(actor.UserId, file.FileName,
            destination.ToArray(), ct);
        return mapper.ToHttpResult(result, StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetAsync(Guid id, CurrentActor actor,
        GetDocumentHandler handler, ResultHttpMapper mapper, CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId,
            new GetDocumentQuery(id), ct));

    private static async Task<IResult> UpdateContentAsync(Guid id,
        UpdateDocumentContentRequest request, CurrentActor actor,
        UpdateDocumentContentHandler handler, ResultHttpMapper mapper,
        CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId, id,
            new UpdateDocumentContentCommand(request.ContentFormat, request.Content,
                request.PlainText, request.ExpectedVersion), ct));

    private static async Task<IResult> RenameAsync(Guid id, RenameDocumentRequest request,
        CurrentActor actor, RenameDocumentHandler handler, ResultHttpMapper mapper,
        CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId, id,
            new RenameDocumentCommand(request.Title, request.ExpectedVersion), ct));

    private static async Task<IResult> DeleteAsync(Guid id, CurrentActor actor,
        DeleteDocumentHandler handler, ResultHttpMapper mapper, CancellationToken ct)
    {
        var result = await handler.HandleAsync(actor.UserId, new DeleteDocumentCommand(id), ct);
        return result.IsSuccess ? Results.NoContent() : mapper.ToHttpResult(result);
    }

    private static bool TryParseScope(string? value, out DocumentScope scope)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            scope = DocumentScope.All;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out scope) &&
            Enum.IsDefined(scope);
    }

    private static IResult FileTooLarge() => ResultHttpMapper.Problem("file_too_large",
        $"The file cannot exceed {StrictTextImportParser.MaxFileBytes} bytes.",
        StatusCodes.Status400BadRequest);
}
