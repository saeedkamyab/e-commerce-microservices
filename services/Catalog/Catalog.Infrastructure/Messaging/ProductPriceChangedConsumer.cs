using Catalog.Infrastructure.Persistence.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Catalog.Infrastructure.Messaging;

internal sealed class ProductPriceChangedConsumer
    : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductPriceChangedConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    public ProductPriceChangedConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<ProductPriceChangedConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
            await factory.CreateConnectionAsync(stoppingToken);

        _channel =
            await _connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        const string queueName =
            "catalog.product-price-changed";

        var consumer =
            new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var messageIdValue =
                    eventArgs.BasicProperties.MessageId;

                if (!Guid.TryParse(
                    messageIdValue,
                    out var messageId))
                {
                    throw new InvalidOperationException(
                        "RabbitMQ message does not contain a valid MessageId.");
                }

                var type =
                    eventArgs.BasicProperties.Type
                    ?? throw new InvalidOperationException(
                        "RabbitMQ message does not contain an event type.");

                var content =
                    Encoding.UTF8.GetString(
                        eventArgs.Body.ToArray());

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
                    eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process product price changed event.");

                await _channel!.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
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

        await base.StopAsync(cancellationToken);
    }
}
