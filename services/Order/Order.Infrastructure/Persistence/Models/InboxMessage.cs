namespace Order.Infrastructure.Persistence.Models;

internal sealed class InboxMessage
{
    public Guid MessageId { get; set; }

    public string Type { get; set; } = null!;

    public DateTime ReceivedOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }
}
