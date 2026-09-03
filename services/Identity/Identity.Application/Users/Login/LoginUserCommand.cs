using MediatR;

namespace Identity.Application.Users.Login;


public sealed record LoginUserCommand(
    string Email,
    string Password)
    : IRequest<AuthenticationResult>;
