using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Infrastructure.Persistence.Inbox;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Payment.Infrastructure.Messaging;

internal sealed class PaymentRequestedConsumer
    : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentRequestedConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public PaymentRequestedConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentRequestedConsumer> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connectionFactory =
            new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password
            };

        _connection =
            await connectionFactory
                .CreateConnectionAsync(stoppingToken);

        _channel =
            await _connection
                .CreateChannelAsync(
                    cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(
                _channel);

        consumer.ReceivedAsync +=
            async (_, args) =>
            {
                try
                {
                    var body =
                        Encoding.UTF8.GetString(
                            args.Body.Span);

                    var messageIdText =
                        args.BasicProperties.MessageId;

                    var type =
                        args.BasicProperties.Type;

                    if (!Guid.TryParse(
                            messageIdText,
                            out var messageId))
                    {
                        throw new InvalidOperationException(
                            "RabbitMQ message id is invalid.");
                    }

                    if (string.IsNullOrWhiteSpace(type))
                    {
                        throw new InvalidOperationException(
                            "RabbitMQ message type is missing.");
                    }

                    using var scope =
                        _scopeFactory.CreateScope();

                    var inboxProcessor =
                        scope.ServiceProvider
                            .GetRequiredService<
                                InboxProcessor>();

                    await inboxProcessor.ProcessAsync(
                        messageId,
                        type,
                        body,
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
                        "Failed to consume PaymentRequested integration event.");

                    await _channel!.BasicNackAsync(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken: stoppingToken);
                }
            };

        await _channel.BasicConsumeAsync(
            queue:
                RabbitMqTopologyInitializer
                    .PaymentRequestedQueueName,
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
