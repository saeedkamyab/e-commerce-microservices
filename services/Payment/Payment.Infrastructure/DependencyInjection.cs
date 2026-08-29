using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Abstractions.Persistence;
using Payment.Infrastructure.Persistence;
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

        return services;
    }
}
