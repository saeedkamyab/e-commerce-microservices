using SharedKernel.Domain;

namespace Payment.Domain.Events;


public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    DateTime OccurredOnUtc)
    : IDomainEvent;
