namespace Dobley.Data.Core.Integrations.RabbitMq;

public interface INotificationMessageConsumer
{
    Task ConsumeAsync(Func<NotificationMessage, CancellationToken, Task> handleMessage,
        CancellationToken cancellationToken);
}
