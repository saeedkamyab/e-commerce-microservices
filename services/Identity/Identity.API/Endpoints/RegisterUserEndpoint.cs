using Identity.Application.Users.Register;
using MediatR;

namespace Identity.API.Endpoints
{
    public static class RegisterUserEndpoint
    {
        public static void MapRegisterUserEndpoint(
            this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/identity/register",
                async (
                    RegisterUserRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command =
                        new RegisterUserCommand(
                            request.Email,
                            request.Password,
                            request.FirstName,
                            request.LastName);

                    var userId =
                        await sender.Send(
                            command,
                            cancellationToken);

                    return Results.Created(
                        $"/api/identity/users/{userId}",
                        new
                        {
                            Id = userId
                        });
                });
        }
    }

    public sealed record RegisterUserRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName);
}
