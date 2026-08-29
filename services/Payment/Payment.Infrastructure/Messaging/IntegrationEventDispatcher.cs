using Payment.Application.Abstractions.Messaging;

namespace Payment.Infrastructure.Messaging;

internal sealed class IntegrationEventDispatcher
{
    private readonly IReadOnlyDictionary<
        string,
        IIntegrationEventHandler> _handlers;

    public IntegrationEventDispatcher(
        IEnumerable<IIntegrationEventHandler> handlers)
    {
        _handlers = handlers.ToDictionary(
            x => x.EventType,
            StringComparer.Ordinal);
    }

    public Task DispatchAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(
                type,
                out var handler))
        {
            throw new InvalidOperationException(
                $"No integration event handler is registered for '{type}'.");
        }

        return handler.HandleAsync(
            messageId,
            type,
            content,
            cancellationToken);
    }
}
