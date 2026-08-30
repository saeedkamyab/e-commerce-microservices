using MediatR;
using Payment.Application.Abstractions.Payments;
using Payment.Application.Abstractions.Persistence;

namespace Payment.Application.Payments.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler
    : IRequestHandler<ProcessPaymentCommand>
{
    private readonly IPaymentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public ProcessPaymentCommandHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment =
            await _repository.GetByIdAsync(
                request.PaymentId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Payment '{request.PaymentId}' was not found.");

        var result =
            await _paymentGateway.ChargeAsync(
                payment.OrderId,
                payment.Amount,
                cancellationToken);

        if (result.IsSuccess)
        {
            payment.MarkSucceeded();
        }
        else
        {
            payment.MarkFailed(
                result.FailureReason
                ?? "Payment failed.");
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
