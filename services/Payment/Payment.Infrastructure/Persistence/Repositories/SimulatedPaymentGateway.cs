using Microsoft.Extensions.Options;
using Payment.Application.Abstractions.Payments;

namespace Payment.Infrastructure.Persistence.Repositories;

internal sealed class SimulatedPaymentGateway
    : IPaymentGateway
{
    private readonly SimulatedPaymentGatewayOptions _options;

    public SimulatedPaymentGateway(
        IOptions<SimulatedPaymentGatewayOptions> options)
    {
        _options = options.Value;
    }

    public Task<PaymentGatewayResult> ChargeAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (_options.AlwaysSucceed)
        {
            return Task.FromResult(
                new PaymentGatewayResult(
                    true,
                    null));
        }

        return Task.FromResult(
            new PaymentGatewayResult(
                false,
                _options.FailureReason));
    }
}
