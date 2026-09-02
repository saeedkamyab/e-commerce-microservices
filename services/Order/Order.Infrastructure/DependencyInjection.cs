using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Messaging.Inventory;
using Order.Infrastructure.Messaging.Payment;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Inbox;
using Order.Infrastructure.Persistence.Outbox;
using Order.Infrastructure.Persistence.Repositories;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("OrderDatabase")
            ?? throw new InvalidOperationException(
                "Order database connection string was not found.");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<OrderDbContext>());


        services.Configure<RabbitMqOptions>(
    configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<
            IIntegrationEventPublisher,
            RabbitMqIntegrationEventPublisher>();

        services.AddScoped<OutboxProcessor>();

        services.AddHostedService<OutboxBackgroundService>();


        services.AddScoped<InboxProcessor>();
        services.AddScoped<IntegrationEventDispatcher>();

        services.AddScoped<
            IIntegrationEventHandler,
            InventoryReservedIntegrationEventHandler>();

        services.AddScoped<
            IIntegrationEventHandler,
            InventoryReservationFailedIntegrationEventHandler>();


        services.AddScoped<
    IIntegrationEventHandler,
    PaymentSucceededIntegrationEventHandler>();

        services.AddScoped<
            IIntegrationEventHandler,
            PaymentFailedIntegrationEventHandler>();

        services.AddSingleton<
    RabbitMqTopologyInitializer>();

        services.AddHostedService<
            RabbitMqTopologyHostedService>();


        services.AddHostedService<
            InventoryResultConsumer>();

        services.AddHostedService<
            PaymentResultConsumer>();


        services.AddScoped<
    IIntegrationEventHandler,
    InventoryReleasedIntegrationEventHandler>();

        return services;
    }
}
