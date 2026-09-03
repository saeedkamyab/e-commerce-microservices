using MediatR;

namespace Identity.Application.Users;


public sealed record GetCurrentUserQuery
    : IRequest<GetCurrentUserResponse>;
