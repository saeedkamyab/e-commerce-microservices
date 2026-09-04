namespace Identity.Domain.Entities;

public sealed class ExternalIdentity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Provider { get; private set; } = null!;

    public string ProviderUserId { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; private set; }

    private ExternalIdentity()
    {
    }

    private ExternalIdentity(
        Guid id,
        Guid userId,
        string provider,
        string providerUserId,
        DateTime createdOnUtc)
    {
        Id = id;
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId;
        CreatedOnUtc = createdOnUtc;
    }

    public static ExternalIdentity Create(
        Guid userId,
        string provider,
        string providerUserId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException(
                "Provider cannot be empty.",
                nameof(provider));
        }

        if (string.IsNullOrWhiteSpace(providerUserId))
        {
            throw new ArgumentException(
                "Provider user id cannot be empty.",
                nameof(providerUserId));
        }

        return new ExternalIdentity(
            Guid.NewGuid(),
            userId,
            provider.Trim(),
            providerUserId.Trim(),
            DateTime.UtcNow);
    }
}
