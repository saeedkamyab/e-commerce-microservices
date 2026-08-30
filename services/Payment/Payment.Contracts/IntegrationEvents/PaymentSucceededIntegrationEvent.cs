namespace Payment.Contracts.IntegrationEvents;

public sealed record PaymentSucceededIntegrationEvent(
 Guid MessageId,
 Guid PaymentId,
 Guid OrderId,
 decimal Amount,
 DateTime OccurredOnUtc);
