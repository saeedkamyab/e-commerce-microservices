using Identity.Application.Abstractions.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.API.Authentication;

internal sealed class CurrentUser
    : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user =
                _httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException(
                    "Current user is not available.");

            var userId =
                user.FindFirstValue(
                    JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(
                    userId,
                    out var id))
            {
                throw new InvalidOperationException(
                    "Current user id is invalid.");
            }

            return id;
        }
    }
}
