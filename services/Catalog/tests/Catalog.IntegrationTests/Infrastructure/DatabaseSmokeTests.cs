namespace Catalog.IntegrationTests.Infrastructure;

[Collection(CatalogDatabaseCollection.Name)]
public sealed class DatabaseSmokeTests
{
    private readonly CatalogDatabaseFixture _fixture;

    public DatabaseSmokeTests(
        CatalogDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Database_Should_Be_Available()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        var canConnect =
            await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}
