using AjaiaDocs.Api.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace AjaiaDocs.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (exception is BadHttpRequestException badRequest)
        {
            logger.LogWarning(exception, "The API request could not be parsed.");
            var statusCode = badRequest.StatusCode is >= 400 and < 500
                ? badRequest.StatusCode
                : StatusCodes.Status400BadRequest;
            await ResultHttpMapper.Problem("invalid_request", "The request is invalid.",
                statusCode).ExecuteAsync(httpContext);
            return true;
        }

        logger.LogError(exception, "An unexpected API failure occurred.");
        await ResultHttpMapper.Problem("unexpected_failure",
            "An unexpected error occurred.", StatusCodes.Status500InternalServerError)
            .ExecuteAsync(httpContext);
        return true;
    }
}
