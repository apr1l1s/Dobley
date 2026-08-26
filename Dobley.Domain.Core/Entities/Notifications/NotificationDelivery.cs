using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class NotificationDelivery
    : IAuditableEntity
{
    public int Id { get; set; }

    public string UserName { get; private set; } = null!;

    public NotificationChannel Channel { get; private set; }

    public string Destination { get; private set; } = null!;

    public int ProductId { get; private set; }

    public DateTime ExpirationDate { get; private set; }

    public string Subject { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    public Product? DomainProduct { get; private set; }

    public User? DomainUser { get; private set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    private NotificationDelivery()
    {
    }

    public static NotificationDelivery Create(string userName, NotificationChannel channel, string destination,
        int productId, DateTime expirationDate, string subject, string body)
    {
        if (userName.IsNullOrEmpty() || userName.Length > 100)
        {
            throw new DomainValidateNotificationException("Логин владельца уведомления должен быть заполнен");
        }

        if (destination.IsNullOrEmpty() || destination.Length > 300)
        {
            throw new DomainValidateNotificationException("Адрес доставки уведомления должен быть заполнен");
        }

        if (productId <= 0)
        {
            throw new DomainValidateNotificationException("Неизвестный продукт для уведомления");
        }

        if (subject.IsNullOrEmpty() || subject.Length > 200)
        {
            throw new DomainValidateNotificationException("Заголовок уведомления должен быть заполнен");
        }

        if (body.IsNullOrEmpty() || body.Length > 2000)
        {
            throw new DomainValidateNotificationException("Текст уведомления должен быть заполнен");
        }

        return new NotificationDelivery
        {
            UserName = userName,
            Channel = channel,
            Destination = destination,
            ProductId = productId,
            ExpirationDate = expirationDate.Date,
            Subject = subject,
            Body = body
        };
    }

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;
}
