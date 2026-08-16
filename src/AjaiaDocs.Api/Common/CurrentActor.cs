using System.Security.Claims;

namespace AjaiaDocs.Api.Common;

public sealed class CurrentActor(IHttpContextAccessor accessor)
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new InvalidOperationException(
                    "The authenticated session does not contain a valid user identifier.");
        }
    }
}
