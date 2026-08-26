using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Workers.Notifications.Options;

public class ExpirationNotificationOptions
{
    public const string DEFAULT_NOTIFICATION_USER_NAME = "admin";
    public const int DEFAULT_NOTIFY_BEFORE_DAYS = 3;

    public NotificationChannel Channel { get; set; } = NotificationChannel.Telegram;

    public string? Destination { get; set; }

    public string NotificationUserName { get; set; } = DEFAULT_NOTIFICATION_USER_NAME;

    public int NotifyBeforeDays { get; set; } = DEFAULT_NOTIFY_BEFORE_DAYS;
}
