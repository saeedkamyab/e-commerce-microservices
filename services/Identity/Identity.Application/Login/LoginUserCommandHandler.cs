using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.Login;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenProvider _accessTokenProvider;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenProvider accessTokenProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _accessTokenProvider = accessTokenProvider;
    }

    public async Task<AuthenticationResult> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var email =
            Email.Create(request.Email);

        var user =
            await _userRepository.GetByEmailAsync(
                email,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        var passwordIsValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordIsValid)
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        var accessToken =
            _accessTokenProvider.Create(user);

        return new AuthenticationResult(
            user.Id,
            accessToken);
    }
}
