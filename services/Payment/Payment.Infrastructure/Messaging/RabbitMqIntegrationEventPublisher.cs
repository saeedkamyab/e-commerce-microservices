using Microsoft.Extensions.Options;
using Payment.Application.Abstractions.Messaging;
using Payment.Contracts.IntegrationEvents;
using RabbitMQ.Client;
using System.Text;

namespace Payment.Infrastructure.Messaging;

internal sealed class RabbitMqIntegrationEventPublisher
    : IIntegrationEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ConnectionFactory _connectionFactory;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqIntegrationEventPublisher(
        IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };
    }

    public async Task PublishAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        await EnsureConnectionAsync(
            cancellationToken);

        var body =
            Encoding.UTF8.GetBytes(content);

        var properties =
            new BasicProperties
            {
                Persistent = true,
                MessageId = messageId.ToString(),
                Type = type,
                ContentType = "application/json"
            };

        var routingKey =
            GetRoutingKey(type);

        await _channel!.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task EnsureConnectionAsync(
        CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } &&
            _channel is { IsOpen: true })
        {
            return;
        }

        _connection =
            await _connectionFactory
                .CreateConnectionAsync(
                    cancellationToken);

        _channel =
            await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private static string GetRoutingKey(
        string type)
    {
        if (type ==
            typeof(PaymentSucceededIntegrationEvent).FullName)
        {
            return "payment.succeeded";
        }

        if (type ==
            typeof(PaymentFailedIntegrationEvent).FullName)
        {
            return "payment.failed";
        }

        throw new InvalidOperationException(
            $"No routing key is configured for integration event '{type}'.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}