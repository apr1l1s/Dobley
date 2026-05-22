using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dobley.Data.Core.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddDataBase(this IServiceCollection services)
    {
        services.AddDbContext<DobleyContext>(options =>
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");

            var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

            options.UseNpgsql(connectionString, // PostgreSQL
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure( // Включение повторных попыток при сбоях
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        );
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // Оптимизация для read-only запросов
        });

        return services;
    }
}