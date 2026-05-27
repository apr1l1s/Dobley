using Dobley.Data.Core.Repositories;
using Dobley.Data.Core.Repositories.Users;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.Repositories.Users;
using Dobley.Domain.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dobley.Data.Core.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services
            .AddCoreServices()
            .AddScoped<IAuthService, AuthService>()
            ;

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddDataBase()
            .AddRepositories()
            .AddMediatr()
            ;

        return services;
    }

    public static IServiceCollection AddMediatr(this IServiceCollection services)
    {
        services
            .AddMediatR(x => x.RegisterServicesFromAssemblies(
                // typeof(IDomainEventNotificationHandler<>).Assembly,
                typeof(IUseCaseDispatcher).Assembly))
            // .AddScoped<IDomainEventDispatcher, DomainEventDispatcher>()
            .AddScoped<IUseCaseDispatcher, UseCaseDispatcher>()
            ;

        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services)
    {
        services.AddDbContext<DobleyContext>(options =>
        {
            options.UseNpgsql(GetConnectionString(), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            });
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<ICommonRepository, CommonRepository>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            ;

        return services;
    }

    private static string GetConnectionString()
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");

        return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }
}