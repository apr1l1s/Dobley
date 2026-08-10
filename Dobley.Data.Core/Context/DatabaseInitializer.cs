using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dobley.Data.Core.Context;

public static class DatabaseInitializer
{
    private const long MIGRATION_LOCK_ID = 90720260810;

    public static async Task MigrateDatabaseAsync(this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DobleyContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DobleyContext>>();
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await context.Database.OpenConnectionAsync(cancellationToken);

            try
            {
                await context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_lock({0})", [MIGRATION_LOCK_ID],
                    cancellationToken);

                logger.LogInformation("Применяются миграции базы данных.");
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Миграции базы данных применены.");
            }
            finally
            {
                await context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock({0})", [MIGRATION_LOCK_ID],
                    cancellationToken);
                await context.Database.CloseConnectionAsync();
            }
        });
    }
}
