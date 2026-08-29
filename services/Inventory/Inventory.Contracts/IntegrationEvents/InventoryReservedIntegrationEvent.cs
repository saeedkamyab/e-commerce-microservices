namespace Inventory.Contracts.IntegrationEvents;


public sealed record InventoryReservedIntegrationEvent(
    Guid MessageId,
    Guid OrderId,
    DateTime OccurredOnUtc);
