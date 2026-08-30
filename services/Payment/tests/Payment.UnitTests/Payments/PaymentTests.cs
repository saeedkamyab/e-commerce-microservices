using Payment.Domain.Enums;

namespace Payment.UnitTests.Payments;

public sealed class PaymentTests
{
    [Fact]
    public void Create_With_Valid_Data_Should_Create_Pending_Payment()
    {
        var orderId = Guid.NewGuid();

        var payment = Domain.Payments.Payment.Create(
            orderId,
            1200m);

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(1200m, payment.Amount);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Null(payment.CompletedOnUtc);
    }

    [Fact]
    public void MarkSucceeded_When_Pending_Should_Mark_Payment_As_Succeeded()
    {
        var payment = Domain.Payments.Payment.Create(
            Guid.NewGuid(),
            1200m);

        payment.MarkSucceeded();

        Assert.Equal(
            PaymentStatus.Succeeded,
            payment.Status);

        Assert.NotNull(payment.CompletedOnUtc);
    }

    [Fact]
    public void MarkFailed_When_Pending_Should_Mark_Payment_As_Failed()
    {
        var payment = Domain.Payments.Payment.Create(
            Guid.NewGuid(),
            1200m);

        payment.MarkFailed("Payment provider rejected the payment.");

        Assert.Equal(
            PaymentStatus.Failed,
            payment.Status);

        Assert.NotNull(payment.CompletedOnUtc);
    }

    [Fact]
    public void MarkSucceeded_When_Already_Succeeded_Should_Throw()
    {
        var payment = Domain.Payments.Payment.Create(
            Guid.NewGuid(),
            1200m);

        payment.MarkSucceeded();

        Assert.Throws<InvalidOperationException>(
            payment.MarkSucceeded);
    }

    [Fact]
    public void MarkFailed_When_Already_Succeeded_Should_Throw()
    {
        var payment = Domain.Payments.Payment.Create(
            Guid.NewGuid(),
            1200m);

        payment.MarkSucceeded();

        Assert.Throws<InvalidOperationException>(
            () => payment.MarkFailed("Payment provider rejected the payment."));
    }
}
