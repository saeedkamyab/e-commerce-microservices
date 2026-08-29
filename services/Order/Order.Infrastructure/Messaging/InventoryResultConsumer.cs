using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Infrastructure.Persistence.Inbox;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Order.Infrastructure.Messaging;

internal sealed class InventoryResultConsumer
    : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InventoryResultConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public InventoryResultConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<InventoryResultConsumer> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
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

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var messageIdString =
                    args.BasicProperties.MessageId;

                var type =
                    args.BasicProperties.Type;

                if (!Guid.TryParse(
                        messageIdString,
                        out var messageId))
                {
                    throw new InvalidOperationException(
                        $"Invalid message id '{messageIdString}'.");
                }

                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new InvalidOperationException(
                        "Integration event type was not provided.");
                }

                var content =
                    Encoding.UTF8.GetString(
                        args.Body.Span);

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
                    "Failed to process inventory result message.");

                if (_channel is { IsOpen: true })
                {
                    await _channel.BasicNackAsync(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken: stoppingToken);
                }
            }
        };

        await _channel.BasicConsumeAsync(
            queue: RabbitMqTopologyInitializer.InventoryResultsQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

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

        await base.StopAsync(
            cancellationToken);
    }
}
