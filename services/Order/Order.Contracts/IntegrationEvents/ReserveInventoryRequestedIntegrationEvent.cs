namespace Order.Contracts.IntegrationEvents;

public sealed record ReserveInventoryRequestedIntegrationEvent(
 Guid MessageId,
 Guid OrderId,
 IReadOnlyCollection<ReserveInventoryItem> Items,
 DateTime OccurredOnUtc);

public sealed record ReserveInventoryItem(
    Guid ProductId,
    int Quantity);
