using Identity.Application.Abstractions.Authentication;

namespace Identity.IntegrationTests.Infrastructure;

public sealed class FakeExternalIdentityProvider
    : IExternalIdentityProvider
{
    public ExternalIdentityResult Result { get; set; } =
        new(
            "Google",
            "google-user-123",
            "user@example.com",
            "Ali",
            "Ahmadi");

    public Task<ExternalIdentityResult> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result);
    }
}
