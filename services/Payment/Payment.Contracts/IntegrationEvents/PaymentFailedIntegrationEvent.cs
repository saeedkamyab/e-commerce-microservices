namespace Payment.Contracts.IntegrationEvents;

public sealed record PaymentFailedIntegrationEvent(
    Guid MessageId,
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    DateTime OccurredOnUtc);
