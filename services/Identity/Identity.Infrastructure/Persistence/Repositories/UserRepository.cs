using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Identity.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository
    : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(
        IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AnyAsync(
                x => x.Email.Value == email.Value,
                cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken);
    }
}
