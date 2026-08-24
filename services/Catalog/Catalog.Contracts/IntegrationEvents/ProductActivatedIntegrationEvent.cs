namespace Catalog.Contracts.IntegrationEvents;

public sealed record ProductActivatedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    DateTime OccurredOnUtc);
