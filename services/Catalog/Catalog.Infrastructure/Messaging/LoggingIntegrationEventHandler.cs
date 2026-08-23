using Catalog.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Messaging;

internal sealed class LoggingIntegrationEventHandler
    : IIntegrationEventHandler
{
    private readonly ILogger<LoggingIntegrationEventHandler> _logger;

    public LoggingIntegrationEventHandler(
        ILogger<LoggingIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling integration event. MessageId: {MessageId}, Type: {Type}",
            messageId,
            type);

        return Task.CompletedTask;
    }
}
