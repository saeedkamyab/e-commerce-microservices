using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Users.Logout;

internal sealed class LogoutCommandHandler
    : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IRefreshTokenProvider refreshTokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenProvider = refreshTokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _refreshTokenProvider.Hash(
                request.RefreshToken);

        var refreshToken =
            await _refreshTokenRepository
                .GetByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        var familyTokens =
            await _refreshTokenRepository
                .GetByFamilyIdAsync(
                    refreshToken.FamilyId,
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
    }
}
