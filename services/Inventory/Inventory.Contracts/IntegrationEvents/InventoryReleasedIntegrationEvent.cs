namespace Inventory.Contracts.IntegrationEvents;

public sealed record InventoryReleasedIntegrationEvent(
    Guid MessageId,
    Guid OrderId,
    DateTime OccurredOnUtc);
