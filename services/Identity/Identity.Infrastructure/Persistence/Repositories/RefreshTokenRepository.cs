using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly IdentityDbContext _dbContext;

    public RefreshTokenRepository(
        IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshToken>> GetByFamilyIdAsync(
    Guid familyId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(x => x.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }
}
