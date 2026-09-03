namespace Identity.Application.Login;

public sealed record AuthenticationResult(
    Guid UserId,
    string AccessToken);
