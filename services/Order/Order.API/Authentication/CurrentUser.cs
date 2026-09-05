using Microsoft.IdentityModel.JsonWebTokens;
using Order.Application.Abstractions.Authentication;

namespace Order.API.Authentication;

internal sealed class CurrentUser : ICurrentUser
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
                    "HttpContext is not available.");

            var subject =
                user.FindFirst(
                    JwtRegisteredClaimNames.Sub)?.Value;

            if (!Guid.TryParse(subject, out var userId))
            {
                throw new UnauthorizedAccessException(
                    "User identifier is missing or invalid.");
            }

            return userId;
        }
    }
}
