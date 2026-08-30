using Payment.Application.Abstractions.Payments;

namespace Payment.IntegrationTests.Fakes;

internal sealed class FakePaymentGateway
  : IPaymentGateway
{
    private readonly PaymentGatewayResult _result;

    public FakePaymentGateway(
        PaymentGatewayResult result)
    {
        _result = result;
    }

    public Task<PaymentGatewayResult> ChargeAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_result);
    }
}
