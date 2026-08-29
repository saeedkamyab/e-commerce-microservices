namespace Inventory.Contracts.IntegrationEvents;

public sealed record InventoryReservationFailedIntegrationEvent(
    Guid MessageId,
    Guid OrderId,
    string Reason,
    DateTime OccurredOnUtc);
