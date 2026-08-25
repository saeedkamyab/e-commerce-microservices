using Catalog.Contracts.IntegrationEvents;
using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.InventoryItems.Commands.CreateInventoryItem;
using MediatR;
using System.Text.Json;

namespace Inventory.Infrastructure.Messaging;

internal sealed class ProductActivatedIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly ISender _sender;

    public ProductActivatedIntegrationEventHandler(
        ISender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        if (type != typeof(ProductActivatedIntegrationEvent).FullName)
        {
            throw new InvalidOperationException(
                $"Unsupported integration event type: '{type}'.");
        }

        var integrationEvent =
            JsonSerializer.Deserialize<ProductActivatedIntegrationEvent>(
                content)
            ?? throw new InvalidOperationException(
                "ProductActivatedIntegrationEvent could not be deserialized.");

        await _sender.Send(
            new CreateInventoryItemCommand(
                integrationEvent.ProductId),
            cancellationToken);
    }
}
