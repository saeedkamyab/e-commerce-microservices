using Identity.Application.Users.Login;
using MediatR;

namespace Identity.Application.Users.ExternalLogin;


public sealed record ExternalLoginCommand(
    string IdToken)
    : IRequest<AuthenticationResult>;
