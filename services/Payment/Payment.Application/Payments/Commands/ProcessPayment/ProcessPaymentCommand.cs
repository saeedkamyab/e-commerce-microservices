using MediatR;

namespace Payment.Application.Payments.Commands.ProcessPayment;

public sealed record ProcessPaymentCommand(
    Guid PaymentId)
    : IRequest;
