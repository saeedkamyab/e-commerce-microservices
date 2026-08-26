using SharedKernel.Domain;

namespace Order.Domain.Events;


public sealed record OrderInventoryReservationStartedDomainEvent(
    Guid OrderId,
    IReadOnlyCollection<InventoryReservationItem> Items,
    DateTime OccurredOnUtc
) : IDomainEvent;

public sealed record InventoryReservationItem(
    Guid ProductId,
    int Quantity);