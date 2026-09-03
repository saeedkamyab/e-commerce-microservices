using MediatR;
namespace Identity.Application.Register;

public sealed record RegisterUserCommand(
  string Email,
  string Password,
  string FirstName,
  string LastName)
  : IRequest<Guid>;
