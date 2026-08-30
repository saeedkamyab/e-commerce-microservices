using Payment.Contracts.IntegrationEvents;
using Payment.Domain.Events;
using SharedKernel.Domain;
using System.Text.Json;

namespace Payment.Infrastructure.Persistence.Outbox;

internal static class OutboxMessageFactory
{
    public static OutboxMessage Create(
        IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            PaymentSucceededDomainEvent e =>
                CreatePaymentSucceeded(e),

            PaymentFailedDomainEvent e =>
                CreatePaymentFailed(e),

            _ => throw new InvalidOperationException(
                $"Unsupported domain event: {domainEvent.GetType().Name}")
        };
    }

    private static OutboxMessage CreatePaymentSucceeded(
        PaymentSucceededDomainEvent domainEvent)
    {
        var integrationEvent =
            new PaymentSucceededIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.PaymentId,
                domainEvent.OrderId,
                domainEvent.Amount,
                domainEvent.OccurredOnUtc);

        return new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = typeof(
                PaymentSucceededIntegrationEvent).FullName!,
            Content = JsonSerializer.Serialize(
                integrationEvent),
            OccurredOnUtc =
                integrationEvent.OccurredOnUtc
        };
    }

    private static OutboxMessage CreatePaymentFailed(
        PaymentFailedDomainEvent domainEvent)
    {
        var integrationEvent =
            new PaymentFailedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.PaymentId,
                domainEvent.OrderId,
                domainEvent.Reason,
                domainEvent.OccurredOnUtc);

        return new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = typeof(
                PaymentFailedIntegrationEvent).FullName!,
            Content = JsonSerializer.Serialize(
                integrationEvent),
            OccurredOnUtc =
                integrationEvent.OccurredOnUtc
        };
    }
}
