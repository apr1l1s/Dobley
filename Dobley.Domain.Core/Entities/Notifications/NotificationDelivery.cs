using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class NotificationDelivery
    : IAuditableEntity
{
    public int Id { get; set; }

    public int NotificationRecipientId { get; private set; }

    public int ProductId { get; private set; }

    public DateTime ExpirationDate { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public NotificationRecipient? DomainNotificationRecipient { get; private set; }

    public Product? DomainProduct { get; private set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    private NotificationDelivery()
    {
    }

    public static NotificationDelivery Create(int notificationRecipientId, int productId,
        DateTime expirationDate, NotificationChannel channel)
    {
        if (notificationRecipientId <= 0)
        {
            throw new DomainValidateNotificationException("Неизвестный получатель уведомлений");
        }

        if (productId <= 0)
        {
            throw new DomainValidateNotificationException("Неизвестный продукт для уведомления");
        }

        return new NotificationDelivery
        {
            NotificationRecipientId = notificationRecipientId,
            ProductId = productId,
            ExpirationDate = expirationDate.Date,
            Channel = channel
        };
    }

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;
}
