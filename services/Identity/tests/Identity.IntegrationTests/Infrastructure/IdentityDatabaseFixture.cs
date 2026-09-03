using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Identity.IntegrationTests.Infrastructure;

public sealed class IdentityDatabaseFixture
    : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("identity_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var dbContext =
            CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    internal IdentityDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(
                    _postgres.GetConnectionString())
                .Options;

        return new IdentityDbContext(options);
    }
}
