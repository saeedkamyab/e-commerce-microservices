namespace Identity.Application.Users;

public sealed record GetCurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName);
