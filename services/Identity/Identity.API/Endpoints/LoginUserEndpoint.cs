using Identity.Application.Login;
using MediatR;

namespace Identity.API.Endpoints
{
    public static class LoginUserEndpoint
    {
        public static void MapLoginUserEndpoint(
            this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/identity/login",
                async (
                    LoginUserRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command =
                        new LoginUserCommand(
                            request.Email,
                            request.Password);

                    var result =
                        await sender.Send(
                            command,
                            cancellationToken);

                    return Results.Ok(
                        new
                        {
                            result.UserId,
                            result.AccessToken
                        });
                });
        }
    }

    public sealed record LoginUserRequest(
        string Email,
        string Password);
}
