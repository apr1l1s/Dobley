namespace Dobley.Data.Core.Integrations.RabbitMq;

public interface INotificationMessagePublisher
{
    Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken);
}
