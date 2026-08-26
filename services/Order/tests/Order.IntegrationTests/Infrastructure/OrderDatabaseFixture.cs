using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Order.IntegrationTests.Infrastructure;

public sealed class OrderDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("order_tests")
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

    internal OrderDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<OrderDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

        return new OrderDbContext(options);
    }
}
