using Microsoft.Extensions.Hosting;

namespace Inventory.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyHostedService
    : IHostedService
{
    private readonly RabbitMqTopologyInitializer _initializer;

    public RabbitMqTopologyHostedService(
        RabbitMqTopologyInitializer initializer)
    {
        _initializer = initializer;
    }

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        return _initializer.InitializeAsync(
            cancellationToken);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}
