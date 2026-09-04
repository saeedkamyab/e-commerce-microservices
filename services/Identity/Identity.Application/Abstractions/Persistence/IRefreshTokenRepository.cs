using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        Identity.Domain.Entities.RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<Identity.Domain.Entities.RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Domain.Entities.RefreshToken>> GetByFamilyIdAsync(
    Guid familyId,
    CancellationToken cancellationToken = default);
}
