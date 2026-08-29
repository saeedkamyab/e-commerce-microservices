namespace Order.Contracts.IntegrationEvents;

public sealed record PaymentRequestedIntegrationEvent(
    Guid MessageId,
    Guid OrderId,
    decimal Amount,
    DateTime OccurredOnUtc);
