namespace Payment.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class PaymentDatabaseCollection
    : ICollectionFixture<PaymentDatabaseFixture>
{
    public const string Name = "PaymentDatabase";
}