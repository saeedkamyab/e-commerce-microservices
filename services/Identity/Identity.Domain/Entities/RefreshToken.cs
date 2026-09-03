namespace Identity.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTime ExpiresOnUtc { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? RevokedOnUtc { get; private set; }

    public bool IsRevoked =>
        RevokedOnUtc.HasValue;

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresOnUtc;

    public bool IsActive =>
        !IsRevoked && !IsExpired;

    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresOnUtc,
        DateTime createdOnUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresOnUtc = expiresOnUtc;
        CreatedOnUtc = createdOnUtc;
    }

  
    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresOnUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException(
                "Token hash cannot be empty.",
                nameof(tokenHash));
        }

        if (expiresOnUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Expiration must be in the future.",
                nameof(expiresOnUtc));
        }

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            tokenHash,
            expiresOnUtc,
            DateTime.UtcNow);
    }

    public void Revoke()
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedOnUtc = DateTime.UtcNow;
    }
}
