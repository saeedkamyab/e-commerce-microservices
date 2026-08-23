namespace Catalog.Contracts.IntegrationEvents;

public sealed record ProductPriceChangedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    string Currency,
    DateTime OccurredOnUtc);