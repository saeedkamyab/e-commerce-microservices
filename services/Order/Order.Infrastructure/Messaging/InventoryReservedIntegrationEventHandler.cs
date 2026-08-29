using Inventory.Contracts.IntegrationEvents;
using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence;
using System.Text.Json;

namespace Order.Infrastructure.Messaging;

internal sealed class InventoryReservedIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryReservedIntegrationEventHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public string EventType =>
        typeof(InventoryReservedIntegrationEvent).FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<
                InventoryReservedIntegrationEvent>(content)
            ?? throw new InvalidOperationException(
                "InventoryReservedIntegrationEvent could not be deserialized.");

        var order =
            await _orderRepository.GetByIdAsync(
                integrationEvent.OrderId,
                cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException(
                $"Order '{integrationEvent.OrderId}' was not found.");
        }

        order.MarkInventoryReserved();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
