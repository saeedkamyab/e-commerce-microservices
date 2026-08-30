using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Payment.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyInitializer
{
    public const string PaymentRequestedQueueName =
        "payment.requested";

    public const string PaymentRequestedRoutingKey =
        "order.payment.requested";

    private readonly RabbitMqOptions _options;
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqTopologyInitializer(
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

    public async Task InitializeAsync(
        CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: PaymentRequestedQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: PaymentRequestedQueueName,
            exchange: _options.Exchange,
            routingKey: PaymentRequestedRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}
