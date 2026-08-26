using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class NotificationInboxMessage
    : IAuditableEntity
{
    public int Id { get; set; }

    public Guid MessageId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Destination { get; private set; } = null!;

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    private NotificationInboxMessage()
    {
    }

    public static NotificationInboxMessage Create(Guid messageId, NotificationChannel channel, string destination)
    {
        if (messageId == Guid.Empty)
        {
            throw new DomainValidateNotificationException("Идентификатор входящего уведомления должен быть заполнен");
        }

        if (destination.IsNullOrEmpty() || destination.Length > 300)
        {
            throw new DomainValidateNotificationException("Адрес доставки уведомления должен быть заполнен");
        }

        return new NotificationInboxMessage
        {
            MessageId = messageId,
            Channel = channel,
            Destination = destination
        };
    }

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;
}
