namespace Dobley.Workers.Notifications.ExpirationNotifications;

public interface IExpirationNotificationPublishingService
{
    Task PublishAsync(CancellationToken cancellationToken);
}
