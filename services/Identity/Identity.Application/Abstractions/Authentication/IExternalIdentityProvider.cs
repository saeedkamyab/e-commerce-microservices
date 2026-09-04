namespace Identity.Application.Abstractions.Authentication;

public interface IExternalIdentityProvider
{
    Task<ExternalIdentityResult> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}

public sealed record ExternalIdentityResult(
    string Provider,
    string ProviderUserId,
    string Email,
    string FirstName,
    string LastName);