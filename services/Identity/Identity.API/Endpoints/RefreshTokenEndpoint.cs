using Identity.Application.RefreshToken;
using MediatR;

namespace Identity.API.Endpoints;

public static class RefreshTokenEndpoint
{
    public static void MapRefreshTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(
"/api/identity/refresh",
async (
    RefreshTokenRequest request,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var result = await sender.Send(
        new RefreshTokenCommand(request.RefreshToken),
        cancellationToken);

    return Results.Ok(new
    {
        result.UserId,
        result.AccessToken,
        result.RefreshToken
    });
});

    }
    public sealed record RefreshTokenRequest(
string RefreshToken);
}
