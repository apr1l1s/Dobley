using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class StorageNotificationSubscription
    : IAuditableEntity, ISoftDeletedEntity
{
    public int Id { get; set; }

    public int NotificationRecipientId { get; private set; }

    public int StorageId { get; private set; }

    public int NotifyBeforeDays { get; private set; }

    public bool IsEnabled { get; private set; }

    public NotificationRecipient? DomainNotificationRecipient { get; private set; }

    public Storage? DomainStorage { get; private set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    public DateTime? DateDeleted { get; private set; }

    public bool IsDeleted => DateDeleted.HasValue;

    private StorageNotificationSubscription()
    {
    }

    public static StorageNotificationSubscription Create(int notificationRecipientId, int storageId,
        int notifyBeforeDays)
    {
        if (notificationRecipientId <= 0)
        {
            throw new DomainValidateNotificationException("Неизвестный получатель уведомлений");
        }

        if (storageId <= 0)
        {
            throw new DomainValidateNotificationException("Неизвестное хранилище для подписки");
        }

        if (notifyBeforeDays is < 0 or > 30)
        {
            throw new DomainValidateNotificationException("Количество дней до уведомления должно быть от 0 до 30");
        }

        return new StorageNotificationSubscription
        {
            NotificationRecipientId = notificationRecipientId,
            StorageId = storageId,
            NotifyBeforeDays = notifyBeforeDays,
            IsEnabled = true
        };
    }

    public void Disable() => IsEnabled = false;

    public void Enable() => IsEnabled = true;

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;

    public void Delete(DateTime dateDeleted) => DateDeleted = dateDeleted;
}
