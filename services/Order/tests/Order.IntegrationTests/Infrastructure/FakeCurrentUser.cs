using Order.Application.Abstractions.Authentication;

namespace Order.IntegrationTests.Infrastructure;

public sealed class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; set; }

    public FakeCurrentUser(Guid userId)
    {
        UserId = userId;
    }
}
