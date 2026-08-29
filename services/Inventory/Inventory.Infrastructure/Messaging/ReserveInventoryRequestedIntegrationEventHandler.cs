using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Abstractions.Persistence;
using Inventory.Application.InventoryItems.Commands.ReserveStock;
using Inventory.Contracts.IntegrationEvents;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Models;
using MediatR;
using Order.Contracts.IntegrationEvents;
using System.Text.Json;

namespace Inventory.Infrastructure.Messaging;

internal sealed class ReserveInventoryRequestedIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly IInventoryItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InventoryDbContext _dbContext;

    public ReserveInventoryRequestedIntegrationEventHandler(
        IInventoryItemRepository repository,
        IUnitOfWork unitOfWork,
        InventoryDbContext dbContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public string EventType =>
        typeof(ReserveInventoryRequestedIntegrationEvent).FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<
                ReserveInventoryRequestedIntegrationEvent>(content)
            ?? throw new InvalidOperationException(
                "Could not deserialize ReserveInventoryRequestedIntegrationEvent.");

        var items =
            new List<(InventoryItem Item, int Quantity)>();

        foreach (var requestedItem in integrationEvent.Items)
        {
            var inventoryItem =
                await _repository.GetByProductIdAsync(
                    requestedItem.ProductId,
                    cancellationToken);

            if (inventoryItem is null)
            {
                await AddFailureOutboxMessageAsync(
                    integrationEvent.OrderId,
                    $"Inventory item for product '{requestedItem.ProductId}' was not found.",
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            items.Add(
                (inventoryItem, requestedItem.Quantity));
        }

        // قبل از تغییر state همه را validate می‌کنیم
        var insufficientItem =
            items.FirstOrDefault(x =>
                x.Quantity > x.Item.AvailableQuantity);

        if (insufficientItem.Item is not null)
        {
            await AddFailureOutboxMessageAsync(
                integrationEvent.OrderId,
                $"Insufficient stock for product '{insufficientItem.Item.ProductId}'.",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        // همه موجودی کافی دارند
        foreach (var item in items)
        {
            item.Item.Reserve(item.Quantity);
        }

        var successEvent =
            new InventoryReservedIntegrationEvent(
                Guid.NewGuid(),
                integrationEvent.OrderId,
                DateTime.UtcNow);

        await AddOutboxMessageAsync(
            successEvent.MessageId,
            typeof(InventoryReservedIntegrationEvent).FullName!,
            JsonSerializer.Serialize(successEvent),
            successEvent.OccurredOnUtc,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task AddFailureOutboxMessageAsync(
        Guid orderId,
        string reason,
        CancellationToken cancellationToken)
    {
        var failureEvent =
            new InventoryReservationFailedIntegrationEvent(
                Guid.NewGuid(),
                orderId,
                reason,
                DateTime.UtcNow);

        await AddOutboxMessageAsync(
            failureEvent.MessageId,
            typeof(InventoryReservationFailedIntegrationEvent).FullName!,
            JsonSerializer.Serialize(failureEvent),
            failureEvent.OccurredOnUtc,
            cancellationToken);
    }

    private async Task AddOutboxMessageAsync(
        Guid id,
        string type,
        string content,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken)
    {
        var message = new OutboxMessage
        {
            Id = id,
            Type = type,
            Content = content,
            OccurredOnUtc = occurredOnUtc
        };

        await _dbContext.OutboxMessages.AddAsync(
            message,
            cancellationToken);
    }
}