using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Data.Core.Integrations.Telegram;
using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Workers.Notifications.Senders;

public class TelegramNotificationSender(ITelegramBotClient telegramBotClient)
    : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Telegram;

    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
        => telegramBotClient.SendMessageAsync(message.Destination, message.Body, cancellationToken);
}
