namespace Payment.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken);
}

public sealed record PaymentGatewayResult(
    bool IsSuccess,
    string? FailureReason);
