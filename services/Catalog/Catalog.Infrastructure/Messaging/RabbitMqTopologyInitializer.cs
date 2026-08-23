using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Catalog.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyInitializer
{
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
            await factory.CreateConnectionAsync(cancellationToken);

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

        const string queueName =
            "catalog.product-price-changed";

        const string routingKey =
            "catalog.product.price-changed";

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.Exchange,
            routingKey: routingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}
