using Identity.Application.Users.Logout;
using MediatR;

namespace Identity.API.Endpoints
{
    public static class LogoutUserEndpoint
    {
        public static void MapLogoutUserEndpoint(
            this IEndpointRouteBuilder app)
        {
            app.MapPost(
    "/api/identity/logout",
    async (
        LogoutRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
    {
        await sender.Send(
            new LogoutCommand(request.RefreshToken),
            cancellationToken);

        return Results.NoContent();
    });
}
        public sealed record LogoutRequest(
            string RefreshToken);
    }
}