using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Abstractions.Persistence;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Inbox;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException(
                "Inventory database connection string was not found.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<InventoryDbContext>());

        services.Configure<RabbitMqOptions>(
    configuration.GetSection(
        RabbitMqOptions.SectionName));

        services.AddScoped<InboxProcessor>();

        services.AddScoped<
            IIntegrationEventHandler,
            ProductActivatedIntegrationEventHandler>();

        services.AddSingleton<RabbitMqTopologyInitializer>();

        services.AddHostedService<
            RabbitMqTopologyHostedService>();

        services.AddHostedService<
            ProductActivatedConsumer>();

        return services;
    }
}
