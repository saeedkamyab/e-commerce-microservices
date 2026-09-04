using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Users.Login;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Users.ExternalLogin;


public sealed class ExternalLoginCommandHandler
    : IRequestHandler<ExternalLoginCommand, AuthenticationResult>
{
    private readonly IExternalIdentityProvider _externalIdentityProvider;
    private readonly IExternalIdentityRepository _externalIdentityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RefreshTokenOptions _refreshTokenOptions;

    public ExternalLoginCommandHandler(
        IExternalIdentityProvider externalIdentityProvider,
        IExternalIdentityRepository externalIdentityRepository,
        IUserRepository userRepository,
        IAccessTokenProvider accessTokenProvider,
        IRefreshTokenProvider refreshTokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IOptions<RefreshTokenOptions> refreshTokenOptions)
    {
        _externalIdentityProvider = externalIdentityProvider;
        _externalIdentityRepository = externalIdentityRepository;
        _userRepository = userRepository;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenProvider = refreshTokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _refreshTokenOptions = refreshTokenOptions.Value;
    }

    public async Task<AuthenticationResult> Handle(
        ExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        var externalResult =
            await _externalIdentityProvider.ValidateAsync(
                request.IdToken,
                cancellationToken);

        var existingExternalIdentity =
            await _externalIdentityRepository.GetAsync(
                externalResult.Provider,
                externalResult.ProviderUserId,
                cancellationToken);

        User user;

        if (existingExternalIdentity is not null)
        {
            user =
                await _userRepository.GetByIdAsync(
                    existingExternalIdentity.UserId,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "External identity references a missing user.");
        }
        else
        {
            var email =
                Email.Create(externalResult.Email);

            user =
                await _userRepository.GetByEmailAsync(
                    email,
                    cancellationToken);

            if (user is null)
            {
                user =
                    User.CreateExternal(
                        email,
                        externalResult.FirstName,
                        externalResult.LastName);

                await _userRepository.AddAsync(
                    user,
                    cancellationToken);
            }

            var newExternalIdentity =
                ExternalIdentity.Create(
                    user.Id,
                    externalResult.Provider,
                    externalResult.ProviderUserId);

            await _externalIdentityRepository.AddAsync(
                newExternalIdentity,
                cancellationToken);
        }

        var accessToken =
            _accessTokenProvider.Create(user);

        var rawRefreshToken =
            _refreshTokenProvider.Generate();

        var refreshToken =
            Domain.Entities.RefreshToken.Create(
                user.Id,
                _refreshTokenProvider.Hash(rawRefreshToken),
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