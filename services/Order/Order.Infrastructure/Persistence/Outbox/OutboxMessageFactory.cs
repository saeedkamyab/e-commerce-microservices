using Order.Contracts.IntegrationEvents;
using Order.Domain.Events;
using Order.Infrastructure.Persistence.Models;
using SharedKernel.Domain;
using System.Text.Json;

namespace Order.Infrastructure.Persistence.Outbox;

internal static class OutboxMessageFactory
{
    public static OutboxMessage Create(
        IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderInventoryReservationStartedDomainEvent e =>
                CreateReserveInventoryRequested(e),

            _ => throw new InvalidOperationException(
                $"Unsupported domain event: {domainEvent.GetType().Name}")
        };
    }

    private static OutboxMessage CreateReserveInventoryRequested(
        OrderInventoryReservationStartedDomainEvent domainEvent)
    {
        var integrationEvent =
            new ReserveInventoryRequestedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OrderId,
                domainEvent.Items
                    .Select(x =>
                        new ReserveInventoryItem(
                            x.ProductId,
                            x.Quantity))
                    .ToArray(),
                domainEvent.OccurredOnUtc);

        return new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = typeof(
                ReserveInventoryRequestedIntegrationEvent).FullName!,
            Content = JsonSerializer.Serialize(
                integrationEvent),
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };
    }
}
