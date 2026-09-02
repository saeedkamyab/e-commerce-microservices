using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Order.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyInitializer
{
    public const string InventoryResultsQueue =
        "order.inventory-results";

    public const string InventoryReservedRoutingKey =
        "inventory.reservation.succeeded";

    public const string InventoryReservationFailedRoutingKey =
        "inventory.reservation.failed";

    public const string PaymentResultsQueue =
    "order.payment-results";

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
            queue: InventoryResultsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: InventoryResultsQueue,
            exchange: _options.Exchange,
            routingKey: InventoryReservedRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: InventoryResultsQueue,
            exchange: _options.Exchange,
            routingKey: InventoryReservationFailedRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);


        await channel.QueueDeclareAsync(
    queue: PaymentResultsQueue,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null,
    cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: PaymentResultsQueue,
            exchange: _options.Exchange,
            routingKey: "payment.succeeded",
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: PaymentResultsQueue,
            exchange: _options.Exchange,
            routingKey: "payment.failed",
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
    queue: PaymentResultsQueue,
    exchange: _options.Exchange,
    routingKey: "inventory.released",
    arguments: null,
    cancellationToken: cancellationToken);

    }
}
