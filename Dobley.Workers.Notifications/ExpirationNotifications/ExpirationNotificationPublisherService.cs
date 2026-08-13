namespace Dobley.Workers.Notifications.ExpirationNotifications;

public class ExpirationNotificationPublisherService(
    IServiceProvider services,
    ILogger<ExpirationNotificationPublisherService> logger)
    : BackgroundService
{
    private readonly TimeSpan _interval = GetInterval();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Expiration notification watcher started with interval {IntervalSeconds} seconds.",
            _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishExpirationNotifications(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Expiration notification publishing failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task PublishExpirationNotifications(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var publishingService = scope.ServiceProvider.GetRequiredService<IExpirationNotificationPublishingService>();

        await publishingService.PublishAsync(cancellationToken);
    }

    private static TimeSpan GetInterval()
        => int.TryParse(Environment.GetEnvironmentVariable("EXPIRATION_WATCH_INTERVAL_SECONDS"), out var seconds)
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromHours(6);
}
