using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Abstractions.Persistence;
using Inventory.Contracts.IntegrationEvents;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Models;
using Order.Contracts.IntegrationEvents;
using System.Text.Json;

namespace Inventory.Infrastructure.Messaging;

internal class ReleaseInventoryRequestedIntegrationEventHandler
 : IIntegrationEventHandler
{
    private readonly IInventoryItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InventoryDbContext _dbContext;

    public ReleaseInventoryRequestedIntegrationEventHandler(
        IInventoryItemRepository repository,
        IUnitOfWork unitOfWork,
        InventoryDbContext dbContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public string EventType =>
        typeof(ReleaseInventoryRequestedIntegrationEvent).FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<ReleaseInventoryRequestedIntegrationEvent>(
                content)
            ?? throw new InvalidOperationException(
                "Could not deserialize ReleaseInventoryRequestedIntegrationEvent.");

        foreach (var item in integrationEvent.Items)
        {
            var inventoryItem =
                await _repository.GetByProductIdAsync(
                    item.ProductId,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Inventory item for product '{item.ProductId}' was not found.");

            inventoryItem.ReleaseReservation(
                item.Quantity);
        }

        var releasedEvent =
            new InventoryReleasedIntegrationEvent(
                Guid.NewGuid(),
                integrationEvent.OrderId,
                DateTime.UtcNow);

        _dbContext.OutboxMessages.Add(
            new OutboxMessage
            {
                Id = releasedEvent.MessageId,
                Type = typeof(
                    InventoryReleasedIntegrationEvent).FullName!,
                Content = JsonSerializer.Serialize(
                    releasedEvent),
                OccurredOnUtc =
                    releasedEvent.OccurredOnUtc
            });

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
