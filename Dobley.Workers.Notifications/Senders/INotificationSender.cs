using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Workers.Notifications.Senders;

public interface INotificationSender
{
    NotificationChannel Channel { get; }

    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken);
}
