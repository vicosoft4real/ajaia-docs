using System.Security.Claims;
using AjaiaDocs.Api.Common;
using AjaiaDocs.Api.Contracts;
using AjaiaDocs.Api.Security;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Core.Users;
using Carter;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AjaiaDocs.Api.Modules.Session;

public sealed class SessionModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var session = app.MapGroup("/api/session");

        session.MapGet("/antiforgery", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken });
        }).AllowAnonymous();

        session.MapGet("", GetSessionAsync).RequireAuthorization();
        session.MapPost("", StartSessionAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .AllowAnonymous();
        session.MapDelete("", EndSessionAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireAuthorization();
    }

    private static async Task<IResult> GetSessionAsync(CurrentActor actor,
        IUserRepository users, ResultHttpMapper mapper, CancellationToken ct)
    {
        var result = await users.GetSeededAsync(actor.UserId, ct);
        return result.IsSuccess
            ? Results.Ok(ToResponse(result.Value))
            : mapper.ToHttpResult(result);
    }

    private static async Task<IResult> StartSessionAsync(StartSessionRequest request,
        HttpContext context, IUserRepository users, ResultHttpMapper mapper,
        CancellationToken ct)
    {
        var result = await users.GetSeededAsync(request.UserId, ct);
        if (!result.IsSuccess)
        {
            return mapper.ToHttpResult(result);
        }

        var user = result.Value;
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName)
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = false
            });
        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> EndSessionAsync(HttpContext context,
        CancellationToken _)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.NoContent();
    }

    private static SessionUserResponse ToResponse(User user) => new(user.Id,
        user.DisplayName, user.Email, user.AvatarColor);
}
