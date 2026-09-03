using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Users.Login;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RefreshTokenOptions _refreshTokenOptions;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenProvider accessTokenProvider,
        IRefreshTokenProvider refreshTokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IOptions<RefreshTokenOptions> refreshTokenOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenProvider = refreshTokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _refreshTokenOptions = refreshTokenOptions.Value;
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

        if (user is null ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        var accessToken =
            _accessTokenProvider.Create(user);

        var rawRefreshToken =
            _refreshTokenProvider.Generate();

        var refreshTokenHash =
            _refreshTokenProvider.Hash(
                rawRefreshToken);

        var refreshToken =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenHash,
                DateTime.UtcNow.AddDays(
                    _refreshTokenOptions.ExpirationDays));

        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new AuthenticationResult(
            user.Id,
            accessToken,
            rawRefreshToken);
    }
}
