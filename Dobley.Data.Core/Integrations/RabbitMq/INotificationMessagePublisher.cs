namespace Dobley.Data.Core.Integrations.RabbitMq;

public interface INotificationMessagePublisher
{
    Task PublishAsync(TelegramNotificationMessage message, CancellationToken cancellationToken);
}
