namespace Inventory.Application.Abstractions.Messaging;

public interface IIntegrationEventHandler
{
    Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken);
}
