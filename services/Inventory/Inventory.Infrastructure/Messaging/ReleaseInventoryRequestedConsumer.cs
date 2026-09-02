using Inventory.Infrastructure.Persistence.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Inventory.Infrastructure.Messaging;

internal class ReleaseInventoryRequestedConsumer
    : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReleaseInventoryRequestedConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public ReleaseInventoryRequestedConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<ReleaseInventoryRequestedConsumer> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
    "ReleaseInventoryRequestedConsumer started.");

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        _connection =
            await factory.CreateConnectionAsync(
                stoppingToken);

        _channel =
            await _connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            _logger.LogInformation(
    "ReleaseInventoryRequested message received.");

            try
            {
                if (!Guid.TryParse(
                    args.BasicProperties.MessageId,
                    out var messageId))
                {
                    throw new InvalidOperationException(
                        "MessageId is missing or invalid.");
                }

                var type =
                    args.BasicProperties.Type
                    ?? throw new InvalidOperationException(
                        "Integration event type is missing.");

                var content =
                    Encoding.UTF8.GetString(
                        args.Body.ToArray());

                using var scope =
                    _scopeFactory.CreateScope();

                var inboxProcessor =
                    scope.ServiceProvider
                        .GetRequiredService<InboxProcessor>();

                await inboxProcessor.ProcessAsync(
                    messageId,
                    type,
                    content,
                    stoppingToken);

                await _channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to consume ReleaseInventoryRequested integration event.");

                await _channel!.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }


        };

        await _channel.BasicConsumeAsync(
    queue: RabbitMqTopologyInitializer.ReleaseInventoryQueueName,
    autoAck: false,
    consumer: consumer,
    cancellationToken: stoppingToken);


        _logger.LogInformation(
    "ReleaseInventoryRequestedConsumer is consuming queue {Queue}",
    RabbitMqTopologyInitializer.ReleaseInventoryQueueName);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}

