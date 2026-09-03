using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Users;

internal sealed class GetCurrentUserQueryHandler
    : IRequestHandler<
        GetCurrentUserQuery,
        GetCurrentUserResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task<GetCurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.GetByIdAsync(
                _currentUser.UserId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Current user was not found.");

        return new GetCurrentUserResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName);
    }
}
