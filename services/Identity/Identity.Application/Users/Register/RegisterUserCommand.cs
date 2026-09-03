using MediatR;
namespace Identity.Application.Users.Register;

public sealed record RegisterUserCommand(
  string Email,
  string Password,
  string FirstName,
  string LastName)
  : IRequest<Guid>;
