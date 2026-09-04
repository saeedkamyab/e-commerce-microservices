using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("IdentityDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'IdentityDatabase' was not found.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(
            sp => sp.GetRequiredService<IdentityDbContext>());
 
        services.AddScoped<IUserRepository, UserRepository>();


        services.AddSingleton<
            IPasswordHasher,
            PasswordHasher>();

        services.Configure<JwtOptions>(
    configuration.GetSection("Jwt"));

        services.AddSingleton<
            IAccessTokenProvider,
            JwtAccessTokenProvider>();

        services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository>();

        services.AddSingleton<
            IRefreshTokenProvider,
            RefreshTokenProvider>();

        services.Configure<RefreshTokenOptions>(
            configuration.GetSection("RefreshToken"));


        services.Configure<GoogleAuthOptions>(
    configuration.GetSection(
        GoogleAuthOptions.SectionName));

        services.AddScoped<
            IExternalIdentityProvider,
            GoogleExternalIdentityProvider>();

        services.AddScoped<
    IExternalIdentityRepository,
    ExternalIdentityRepository>();


        return services;
    }
}
