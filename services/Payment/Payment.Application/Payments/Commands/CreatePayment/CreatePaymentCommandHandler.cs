using MediatR;
using Payment.Application.Abstractions.Persistence;

namespace Payment.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment =
            Domain.Payments.Payment.Create(
                request.OrderId,
                request.Amount);

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return payment.Id;
    }
}
