using Catalog.Contracts.IntegrationEvents;
using Catalog.Domain.Events;
using Catalog.Infrastructure.Persistence.Models;
using SharedKernel.Domain;
using System.Text.Json;

namespace Catalog.Infrastructure.Persistence.Outbox;

internal static class OutboxMessageFactory
{
    public static OutboxMessage? Create(
        IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ProductPriceChangedDomainEvent priceChanged =>
                CreatePriceChangedMessage(priceChanged),

            _ => null
        };
    }

    private static OutboxMessage CreatePriceChangedMessage(
        ProductPriceChangedDomainEvent domainEvent)
    {
        var integrationEvent =
     new ProductPriceChangedIntegrationEvent(
         Guid.NewGuid(),
         domainEvent.ProductId,
         domainEvent.OldPrice,
         domainEvent.NewPrice,
         domainEvent.Currency,
         domainEvent.OccurredOnUtc);

        return new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = typeof(ProductPriceChangedIntegrationEvent).FullName!,
            Content = JsonSerializer.Serialize(integrationEvent),
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };
    }
}
