using Identity.Application.Abstractions.Persistence;
using Identity.Application.Common.Exceptions;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace Identity.Infrastructure.Persistence;

public  class IdentityDbContext
 : DbContext, IUnitOfWork
{
    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens =>
    Set<RefreshToken>();
    public DbSet<ExternalIdentity> ExternalIdentities =>
    Set<ExternalIdentity>();

    public async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
                "A concurrency conflict occurred.");
        }
        catch (DbUpdateException ex)
            when (IsEmailUniqueViolation(ex))
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }
    }
    private static bool IsEmailUniqueViolation(
    DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ux_users_email"
        };
    }
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDbContext).Assembly);
    }
}
