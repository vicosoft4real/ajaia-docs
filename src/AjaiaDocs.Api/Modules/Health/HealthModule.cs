using Carter;

namespace AjaiaDocs.Api.Modules.Health;

public sealed class HealthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .AllowAnonymous();
}
