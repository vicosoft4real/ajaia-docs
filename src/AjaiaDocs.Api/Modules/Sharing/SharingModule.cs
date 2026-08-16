using AjaiaDocs.Api.Common;
using AjaiaDocs.Api.Contracts;
using AjaiaDocs.Api.Security;
using AjaiaDocs.Application.Features.Sharing.GetShareCandidates;
using AjaiaDocs.Application.Features.Sharing.GrantShare;
using AjaiaDocs.Application.Features.Sharing.ListShares;
using AjaiaDocs.Application.Features.Sharing.RevokeShare;
using Carter;

namespace AjaiaDocs.Api.Modules.Sharing;

public sealed class SharingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/share-candidates", GetCandidatesAsync)
            .RequireAuthorization();

        var shares = app.MapGroup("/api/documents/{id:guid}/shares")
            .RequireAuthorization();
        shares.MapGet("", ListAsync);
        shares.MapPost("", GrantAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        shares.MapDelete("/{userId:guid}", RevokeAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
    }

    private static async Task<IResult> GetCandidatesAsync(Guid documentId,
        CurrentActor actor, GetShareCandidatesHandler handler, ResultHttpMapper mapper,
        CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId, documentId, ct));

    private static async Task<IResult> ListAsync(Guid id, CurrentActor actor,
        ListDocumentSharesHandler handler, ResultHttpMapper mapper, CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId, id, ct));

    private static async Task<IResult> GrantAsync(Guid id, GrantShareRequest request,
        CurrentActor actor, GrantDocumentShareHandler handler, ResultHttpMapper mapper,
        CancellationToken ct) =>
        mapper.ToHttpResult(await handler.HandleAsync(actor.UserId, id, request.UserId, ct),
            StatusCodes.Status201Created);

    private static async Task<IResult> RevokeAsync(Guid id, Guid userId,
        CurrentActor actor, RevokeDocumentShareHandler handler, ResultHttpMapper mapper,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(actor.UserId, id, userId, ct);
        return result.IsSuccess ? Results.NoContent() : mapper.ToHttpResult(result);
    }
}
