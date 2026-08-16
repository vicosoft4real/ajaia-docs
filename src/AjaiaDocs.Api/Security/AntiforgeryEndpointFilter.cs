using AjaiaDocs.Api.Common;
using Microsoft.AspNetCore.Antiforgery;

namespace AjaiaDocs.Api.Security;

public sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return ResultHttpMapper.Problem("antiforgery_validation_failed",
                "The antiforgery token is missing or invalid.",
                StatusCodes.Status400BadRequest);
        }

        return await next(context);
    }
}
