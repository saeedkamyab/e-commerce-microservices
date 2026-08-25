using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Inventory.IntegrationTests.Infrastructure;

public sealed class InventoryDatabaseFixture
   : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("inventory_tests")
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

    internal InventoryDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

        return new InventoryDbContext(options);
    }
}
