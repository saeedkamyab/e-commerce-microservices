using Identity.Application.Users.Login;
using MediatR;

namespace Identity.Application.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : IRequest<AuthenticationResult>;
