namespace Dobley.Data.Core.Integrations.RabbitMq;

public interface INotificationMessageConsumer
{
    Task ConsumeAsync(Func<TelegramNotificationMessage, CancellationToken, Task> handleMessage,
        CancellationToken cancellationToken);
}
