using Identity.Application.Users;
using MediatR;

namespace Identity.API.Endpoints
{
    public static class GetCurrentUserEndpoint
    {
        public static void MapGetCurrentUserEndpoint(
            this IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/identity/me",
                    async (
                        ISender sender,
                        CancellationToken cancellationToken) =>
                    {
                        var result =
                            await sender.Send(
                                new GetCurrentUserQuery(),
                                cancellationToken);

                        return Results.Ok(result);
                    })
                .RequireAuthorization();
        }
    }
}
