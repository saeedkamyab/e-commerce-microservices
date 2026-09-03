namespace Identity.Application.Users.Login;

public sealed record AuthenticationResult(
    Guid UserId,
    string AccessToken,
    string RefreshToken);
