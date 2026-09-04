using MediatR;

namespace Identity.Application.Users.Logout;

public sealed record LogoutCommand(
   string RefreshToken)
   : IRequest;
