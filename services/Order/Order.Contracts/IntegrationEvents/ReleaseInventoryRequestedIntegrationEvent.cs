namespace Order.Contracts.IntegrationEvents;

public sealed record ReleaseInventoryRequestedIntegrationEvent(
 Guid MessageId,
 Guid OrderId,
 IReadOnlyCollection<ReleaseInventoryItem> Items,
 DateTime OccurredOnUtc);

public sealed record ReleaseInventoryItem(
    Guid ProductId,
    int Quantity);
