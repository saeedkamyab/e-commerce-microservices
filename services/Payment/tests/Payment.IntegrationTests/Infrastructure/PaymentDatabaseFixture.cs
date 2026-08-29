using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Payment.IntegrationTests.Infrastructure;

public sealed class PaymentDatabaseFixture
: IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("payment_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    internal PaymentDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<PaymentDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

        return new PaymentDbContext(options);
    }
}