namespace Payment.Application.Abstractions.Messaging;

public interface IIntegrationEventHandler
{
    string EventType { get; }

    Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken);
}
