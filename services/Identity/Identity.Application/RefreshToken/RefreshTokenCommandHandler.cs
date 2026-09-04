using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Users.Login;
using MediatR;
using Microsoft.Extensions.Options;



namespace Identity.Application.RefreshToken;

internal sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenProvider refreshTokenProvider,
        IAccessTokenProvider accessTokenProvider,
        IUnitOfWork unitOfWork,
        IOptions<RefreshTokenOptions> options)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenProvider = refreshTokenProvider;
        _accessTokenProvider = accessTokenProvider;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<AuthenticationResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _refreshTokenProvider.Hash(
                request.RefreshToken);

        var currentRefreshToken =
            await _refreshTokenRepository
                .GetByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        if (currentRefreshToken is null )
        {
            throw new InvalidOperationException(
                "Invalid refresh token.");
        }
        if (currentRefreshToken.IsExpired)
        {
            throw new InvalidOperationException(
                "Invalid refresh token.");
        }

        if (currentRefreshToken.IsRevoked)
        {
            var familyTokens =
                await _refreshTokenRepository.GetByFamilyIdAsync(
                    currentRefreshToken.FamilyId,
                    cancellationToken);

            foreach (var token in familyTokens)
            {
                if (!token.IsRevoked)
                {
                    token.Revoke();
                }
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            throw new InvalidOperationException(
                "Invalid refresh token.");
        }


        var user =
            await _userRepository.GetByIdAsync(
                currentRefreshToken.UserId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Invalid refresh token.");

        var newRawRefreshToken =
            _refreshTokenProvider.Generate();

        var newRefreshTokenHash =
            _refreshTokenProvider.Hash(
                newRawRefreshToken);

        var newRefreshToken =
     Domain.Entities.RefreshToken.Create(
         user.Id,
         currentRefreshToken.FamilyId,
         newRefreshTokenHash,
         DateTime.UtcNow.AddDays(
             _options.ExpirationDays));

        currentRefreshToken.ReplaceWith(
    newRefreshToken.Id);

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        var accessToken =
            _accessTokenProvider.Create(user);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new AuthenticationResult(
            user.Id,
            accessToken,
            newRawRefreshToken);
    }
}
