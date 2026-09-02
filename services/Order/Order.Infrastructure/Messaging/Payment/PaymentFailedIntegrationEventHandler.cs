using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence;
using Payment.Contracts.IntegrationEvents;
using System.Text.Json;

namespace Order.Infrastructure.Messaging.Payment;

internal sealed class PaymentFailedIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentFailedIntegrationEventHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public string EventType =>
        typeof(PaymentFailedIntegrationEvent).FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(
                content)
            ?? throw new InvalidOperationException(
                "Could not deserialize PaymentFailedIntegrationEvent.");

        var order =
            await _repository.GetByIdAsync(
                integrationEvent.OrderId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Order '{integrationEvent.OrderId}' was not found.");

        order.MarkPaymentFailed();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
