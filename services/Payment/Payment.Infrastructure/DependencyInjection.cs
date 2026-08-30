using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Abstractions.Messaging;
using Payment.Application.Abstractions.Persistence;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Inbox;
using Payment.Infrastructure.Persistence.Outbox;
using Payment.Infrastructure.Persistence.Repositories;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PaymentDatabase")
            ?? throw new InvalidOperationException(
                "Payment database connection string was not found.");

        services.AddDbContext<PaymentDbContext>(
            options =>
                options.UseNpgsql(connectionString));

        services.AddScoped<
            IPaymentRepository,
            PaymentRepository>();

        services.AddScoped<IUnitOfWork>(
            sp => sp.GetRequiredService<PaymentDbContext>());

        services.AddScoped<InboxProcessor>();

        services.AddScoped<
            IIntegrationEventHandler,
            PaymentRequestedIntegrationEventHandler>();

        services.AddScoped<IntegrationEventDispatcher>();

        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<
            RabbitMqTopologyInitializer>();

        services.AddHostedService<
            RabbitMqTopologyHostedService>();

        services.AddHostedService<
            PaymentRequestedConsumer>();


        services.AddSingleton<
    IIntegrationEventPublisher,
    RabbitMqIntegrationEventPublisher>();

        services.AddScoped<OutboxProcessor>();

        services.AddHostedService<
            OutboxBackgroundService>();

        return services;
    }
}
