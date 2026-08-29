using Payment.Domain.Enums;

namespace Payment.Domain.Payments;

public sealed class Payment
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }

    private Payment()
    {
    }

    private Payment(
        Guid id,
        Guid orderId,
        decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException(
                "Order id cannot be empty.",
                nameof(orderId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Payment amount must be greater than zero.");

        Id = id;
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedOnUtc = DateTime.UtcNow;
    }
    public static Payment Create(
       Guid orderId,
       decimal amount)
    {
        return new Payment(
            Guid.NewGuid(),
            orderId,
            amount);
    }

    public void MarkSucceeded()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot succeed payment in status {Status}.");

        Status = PaymentStatus.Succeeded;
        CompletedOnUtc = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot fail payment in status {Status}.");

        Status = PaymentStatus.Failed;
        CompletedOnUtc = DateTime.UtcNow;
    }
}
