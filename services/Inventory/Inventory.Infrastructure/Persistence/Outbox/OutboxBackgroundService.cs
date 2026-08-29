using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Persistence.Outbox;

internal class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;

    public OutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<OutboxProcessor>();

                await processor.ProcessAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while processing outbox messages.");
            }
        }
    }
}