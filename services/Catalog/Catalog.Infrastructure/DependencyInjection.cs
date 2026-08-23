using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Abstractions.Persistence.Queries;
using Catalog.Infrastructure.Messaging;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Inbox;
using Catalog.Infrastructure.Persistence.Outbox;
using Catalog.Infrastructure.Persistence.Queries;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CatalogDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'CatalogDatabase' was not found.");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadService, ProductReadService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<CatalogDbContext>());


        services.Configure<RabbitMqOptions>(
    configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<
            IIntegrationEventPublisher,
            RabbitMqIntegrationEventPublisher>();

        services.AddSingleton<RabbitMqTopologyInitializer>();
        services.AddHostedService<RabbitMqTopologyHostedService>();

        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();

        services.AddHostedService<ProductPriceChangedConsumer>();


        services.AddScoped<InboxProcessor>();

        services.AddScoped<
            IIntegrationEventHandler,
            LoggingIntegrationEventHandler>();

        return services;
    }
}
