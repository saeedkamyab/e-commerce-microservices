using Identity.Application.Abstractions.Authentication;
using Identity.Domain.Entities;

namespace Identity.IntegrationTests.Infrastructure;

public sealed class FakeAccessTokenProvider
    : IAccessTokenProvider
{
    public string Create(
        User user)
    {
        return $"fake-token-{user.Id}";
    }
}
