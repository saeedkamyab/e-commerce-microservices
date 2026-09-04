using Google.Apis.Auth;
using Identity.Application.Abstractions.Authentication;
using Identity.Application.Common.Exceptions;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Authentication;

internal class GoogleExternalIdentityProvider : IExternalIdentityProvider
{
    private readonly GoogleAuthOptions _options;

    public GoogleExternalIdentityProvider(
        IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<ExternalIdentityResult> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedException(
                "Invalid external identity token.");
        }

        try
        {
            var payload =
                await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [_options.ClientId]
                    });

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new UnauthorizedException(
                    "Google account does not contain an email.");
            }

            var firstName =
    !string.IsNullOrWhiteSpace(payload.GivenName)
        ? payload.GivenName
        : payload.Name ?? "Google User";

            var lastName =
                !string.IsNullOrWhiteSpace(payload.FamilyName)
                    ? payload.FamilyName
                    : "-";


            return new ExternalIdentityResult(
    "Google",
    payload.Subject,
    payload.Email,
    firstName,
    lastName);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException(
                "Invalid external identity token.");
        }
    }
}