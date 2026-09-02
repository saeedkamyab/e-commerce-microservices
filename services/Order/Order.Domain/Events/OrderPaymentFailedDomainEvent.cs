using SharedKernel.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Domain.Events;

public sealed record OrderPaymentFailedDomainEvent(
    Guid OrderId,
    IReadOnlyCollection<InventoryReservationItem> Items,
    DateTime OccurredOnUtc)
    : IDomainEvent;
