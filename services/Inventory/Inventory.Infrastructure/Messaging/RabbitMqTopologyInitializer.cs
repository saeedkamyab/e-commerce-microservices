using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Inventory.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyInitializer
{
    public const string QueueName =
        "inventory.product-activated";

    public const string RoutingKey =
        "catalog.product.activated";

    private readonly RabbitMqOptions _options;

    public RabbitMqTopologyInitializer(
        IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        await using var connection =
            await factory.CreateConnectionAsync(
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
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: _options.Exchange,
            routingKey: RoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}
