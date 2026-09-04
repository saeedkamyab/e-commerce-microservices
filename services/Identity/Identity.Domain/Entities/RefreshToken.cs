namespace Identity.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid FamilyId { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

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
        Guid familyId,
        string tokenHash,
        DateTime expiresOnUtc,
        DateTime createdOnUtc)
    {
        Id = id;
        UserId = userId;
        FamilyId = familyId;
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
            Guid.NewGuid(),
            tokenHash,
            expiresOnUtc,
            DateTime.UtcNow);
    }
    public static RefreshToken Create(
    Guid userId,
    Guid familyId,
    string tokenHash,
    DateTime expiresOnUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));

        if (familyId == Guid.Empty)
            throw new ArgumentException(
                "Family id cannot be empty.",
                nameof(familyId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException(
                "Token hash cannot be empty.",
                nameof(tokenHash));

        if (expiresOnUtc <= DateTime.UtcNow)
            throw new ArgumentException(
                "Expiration must be in the future.",
                nameof(expiresOnUtc));

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            familyId,
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
    public void ReplaceWith(Guid replacementTokenId)
    {
        if (replacementTokenId == Guid.Empty)
            throw new ArgumentException(
                "Replacement token id cannot be empty.",
                nameof(replacementTokenId));

        if (IsRevoked)
            throw new InvalidOperationException(
                "Refresh token is already revoked.");

        RevokedOnUtc = DateTime.UtcNow;
        ReplacedByTokenId = replacementTokenId;
    }
}
