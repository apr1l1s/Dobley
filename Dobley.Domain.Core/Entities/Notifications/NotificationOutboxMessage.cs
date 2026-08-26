using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class NotificationOutboxMessage
    : IAuditableEntity
{
    public int Id { get; set; }

    public Guid MessageId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Destination { get; private set; } = null!;

    public string Subject { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    public int AttemptCount { get; private set; }

    public DateTime? DateProcessed { get; private set; }

    public string? Error { get; private set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    private NotificationOutboxMessage()
    {
    }

    public static NotificationOutboxMessage Create(NotificationChannel channel, string destination, string subject,
        string body)
    {
        ValidateMessage(destination, subject, body);

        return new NotificationOutboxMessage
        {
            MessageId = Guid.NewGuid(),
            Channel = channel,
            Destination = destination,
            Subject = subject,
            Body = body
        };
    }

    public void MarkFailed(string error)
    {
        AttemptCount++;
        Error = error[..Math.Min(error.Length, 2000)];
    }

    public void MarkPublished(DateTime dateProcessed)
    {
        AttemptCount++;
        DateProcessed = dateProcessed;
        Error = null;
    }

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;

    private static void ValidateMessage(string destination, string subject, string body)
    {
        if (destination.IsNullOrEmpty() || destination.Length > 300)
        {
            throw new DomainValidateNotificationException("Адрес доставки уведомления должен быть заполнен");
        }

        if (subject.IsNullOrEmpty() || subject.Length > 200)
        {
            throw new DomainValidateNotificationException("Заголовок уведомления должен быть заполнен");
        }

        if (body.IsNullOrEmpty() || body.Length > 2000)
        {
            throw new DomainValidateNotificationException("Текст уведомления должен быть заполнен");
        }
    }
}
