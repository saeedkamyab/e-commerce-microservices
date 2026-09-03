namespace Identity.Application.Abstractions.Authentication;

public sealed class RefreshTokenOptions
{
    public int ExpirationDays { get; init; } = 7;
}
