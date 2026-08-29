using MediatR;

namespace Payment.Application.Payments.Commands.CreatePayment;


public sealed record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount)
    : IRequest<Guid>;
