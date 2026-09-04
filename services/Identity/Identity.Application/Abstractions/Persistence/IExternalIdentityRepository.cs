using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface IExternalIdentityRepository
{
    Task<ExternalIdentity?> GetAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ExternalIdentity externalIdentity,
        CancellationToken cancellationToken = default);
}
