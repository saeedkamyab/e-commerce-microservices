using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;

namespace Identity.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
    Email email,
    CancellationToken cancellationToken = default);


    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);
}
