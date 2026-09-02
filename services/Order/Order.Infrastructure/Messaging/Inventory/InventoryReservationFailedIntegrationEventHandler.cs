using Inventory.Contracts.IntegrationEvents;
using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence;
using System.Text.Json;

namespace Order.Infrastructure.Messaging.Inventory;

internal sealed class InventoryReservationFailedIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryReservationFailedIntegrationEventHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public string EventType =>
        typeof(InventoryReservationFailedIntegrationEvent).FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<
                InventoryReservationFailedIntegrationEvent>(content)
            ?? throw new InvalidOperationException(
                "InventoryReservationFailedIntegrationEvent could not be deserialized.");

        var order =
            await _orderRepository.GetByIdAsync(
                integrationEvent.OrderId,
                cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException(
                $"Order '{integrationEvent.OrderId}' was not found.");
        }

        order.MarkInventoryFailed();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
