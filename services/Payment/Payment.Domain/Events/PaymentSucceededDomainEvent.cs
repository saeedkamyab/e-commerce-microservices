using SharedKernel.Domain;

namespace Payment.Domain.Events;

public sealed record PaymentSucceededDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime OccurredOnUtc)
    : IDomainEvent;
