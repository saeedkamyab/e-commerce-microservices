namespace Payment.Infrastructure.Persistence.Repositories;

internal sealed class SimulatedPaymentGatewayOptions
{
    public bool AlwaysSucceed { get; init; } = true;

    public string FailureReason { get; init; } =
        "Simulated payment failure.";
}
