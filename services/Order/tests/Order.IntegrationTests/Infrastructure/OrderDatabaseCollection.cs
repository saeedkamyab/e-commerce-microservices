namespace Order.IntegrationTests.Infrastructure;


[CollectionDefinition(Name)]
public sealed class OrderDatabaseCollection
    : ICollectionFixture<OrderDatabaseFixture>
{
    public const string Name = "OrderDatabase";
}
