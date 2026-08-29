using SharedKernel.Domain;

namespace Order.Domain.Events;

public sealed record OrderPaymentRequestedDomainEvent(
    Guid OrderId,
    decimal Amount,
    DateTime OccurredOnUtc) : IDomainEvent;
