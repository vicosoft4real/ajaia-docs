using AjaiaDocs.Api.Common;

namespace AjaiaDocs.Api.Middleware;

public sealed class ApiBindingFailureMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (!context.Request.Path.StartsWithSegments("/api") ||
            context.Response.HasStarted ||
            context.Response.ContentType is not null ||
            context.Response.StatusCode is not (StatusCodes.Status400BadRequest or
                StatusCodes.Status415UnsupportedMediaType))
        {
            return;
        }

        await ResultHttpMapper.Problem("invalid_request", "The request is invalid.",
            context.Response.StatusCode).ExecuteAsync(context);
    }
}
