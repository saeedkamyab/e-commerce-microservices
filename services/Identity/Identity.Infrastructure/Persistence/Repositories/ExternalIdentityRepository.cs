using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

internal sealed class ExternalIdentityRepository
    : IExternalIdentityRepository
{
    private readonly IdentityDbContext _dbContext;

    public ExternalIdentityRepository(
        IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ExternalIdentity?> GetAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ExternalIdentities
            .SingleOrDefaultAsync(
                x =>
                    x.Provider == provider &&
                    x.ProviderUserId == providerUserId,
                cancellationToken);
    }

    public async Task AddAsync(
        ExternalIdentity externalIdentity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ExternalIdentities.AddAsync(
            externalIdentity,
            cancellationToken);
    }
}
