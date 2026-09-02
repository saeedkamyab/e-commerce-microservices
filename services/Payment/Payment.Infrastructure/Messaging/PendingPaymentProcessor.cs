using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions.Persistence;
using Payment.Application.Payments.Commands.ProcessPayment;

namespace Payment.Infrastructure.Messaging;

internal sealed class PendingPaymentProcessor
  : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingPaymentProcessor> _logger;

    public PendingPaymentProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingPaymentProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer =
            new PeriodicTimer(
                TimeSpan.FromSeconds(3));

        while (await timer.WaitForNextTickAsync(
                   stoppingToken))
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IPaymentRepository>();

                var mediator =
                    scope.ServiceProvider
                        .GetRequiredService<IMediator>();

                var pendingPayments =
                    await repository.GetPendingAsync(
                        batchSize: 20,
                        stoppingToken);

                foreach (var payment in pendingPayments)
                {
                    try
                    {
                        await mediator.Send(
                            new ProcessPaymentCommand(
                                payment.Id),
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to process Payment {PaymentId}.",
                            payment.Id);
                    }
                }
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
                    "Failed to process pending payments.");
            }
        }
    }
}
