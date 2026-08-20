using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Catalog.IntegrationTests.Infrastructure;

public sealed class CatalogDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("catalog_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public string ConnectionString =>
        _postgresContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    public CatalogDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        return new CatalogDbContext(options);
    }
}
