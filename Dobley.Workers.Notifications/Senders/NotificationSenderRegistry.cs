using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Workers.Notifications.Senders;

public class NotificationSenderRegistry(IEnumerable<INotificationSender> senders)
{
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationSender> _senders =
        senders.ToDictionary(x => x.Channel);

    public INotificationSender GetSender(NotificationChannel channel)
    {
        if (_senders.TryGetValue(channel, out var sender))
        {
            return sender;
        }

        throw new InvalidOperationException($"Канал уведомлений '{channel}' не настроен.");
    }
}
