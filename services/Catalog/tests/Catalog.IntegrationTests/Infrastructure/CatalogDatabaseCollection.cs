namespace Catalog.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class CatalogDatabaseCollection
    : ICollectionFixture<CatalogDatabaseFixture>
{
    public const string Name = "CatalogDatabase";
}
