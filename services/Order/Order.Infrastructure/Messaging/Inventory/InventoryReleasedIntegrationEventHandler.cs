using Inventory.Contracts.IntegrationEvents;
using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence;
using System.Text.Json;

namespace Order.Infrastructure.Messaging.Inventory;

internal class InventoryReleasedIntegrationEventHandler
  : IIntegrationEventHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryReleasedIntegrationEventHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public string EventType =>
        typeof(InventoryReleasedIntegrationEvent).FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<InventoryReleasedIntegrationEvent>(
                content)
            ?? throw new InvalidOperationException(
                "Could not deserialize InventoryReleasedIntegrationEvent.");

        var order =
            await _repository.GetByIdAsync(
                integrationEvent.OrderId,

                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Order '{integrationEvent.OrderId}' was not found.");

        order.MarkCancelledAfterInventoryRelease();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
