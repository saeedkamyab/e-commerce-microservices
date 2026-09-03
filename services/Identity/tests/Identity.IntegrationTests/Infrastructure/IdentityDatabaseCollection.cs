namespace Identity.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IdentityDatabaseCollection
    : ICollectionFixture<IdentityDatabaseFixture>
{
    public const string Name =
        "IdentityDatabase";
}
