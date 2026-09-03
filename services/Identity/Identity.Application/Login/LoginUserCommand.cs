using MediatR;

namespace Identity.Application.Login;


public sealed record LoginUserCommand(
    string Email,
    string Password)
    : IRequest<AuthenticationResult>;
