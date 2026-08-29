using Order.Contracts.IntegrationEvents;
using Payment.Application.Abstractions.Messaging;
using Payment.Application.Abstractions.Persistence;
using System.Text.Json;

namespace Payment.Infrastructure.Messaging;

internal sealed class PaymentRequestedIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly IPaymentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentRequestedIntegrationEventHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public string EventType =>
        typeof(PaymentRequestedIntegrationEvent)
            .FullName!;

    public async Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<
                PaymentRequestedIntegrationEvent>(
                    content)
            ?? throw new InvalidOperationException(
                "PaymentRequestedIntegrationEvent could not be deserialized.");

        var payment =
            Domain.Payments.Payment.Create(
                integrationEvent.OrderId,
                integrationEvent.Amount);

        await _repository.AddAsync(
            payment,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
