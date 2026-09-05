using Identity.Application.Users.ExternalLogin;
using MediatR;

namespace Identity.API.Endpoints
{
    public static class GoogleLoginEndpoint
    {
        public static IEndpointRouteBuilder MapGoogleLogin(
            this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/identity/google-login",
                async (
                    GoogleLoginRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result =
                        await sender.Send(
                            new ExternalLoginCommand(
                                request.IdToken),
                            cancellationToken);

                    return Results.Ok(result);
                });

            return app;
        }
    }

    public sealed record GoogleLoginRequest(
        string IdToken);
}
