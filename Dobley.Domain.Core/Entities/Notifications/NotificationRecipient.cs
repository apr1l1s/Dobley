using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class NotificationRecipient
    : IAuditableEntity, ISoftDeletedEntity
{
    public int Id { get; set; }

    public string UserName { get; private set; } = null!;

    public NotificationChannel Channel { get; private set; }

    public string ExternalId { get; private set; } = null!;

    public string? DisplayName { get; private set; }

    public User? DomainUser { get; private set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    public DateTime? DateDeleted { get; private set; }

    public bool IsDeleted => DateDeleted.HasValue;

    private NotificationRecipient()
    {
    }

    public static NotificationRecipient Create(string userName, NotificationChannel channel, string externalId,
        string? displayName)
    {
        if (userName.IsNullOrEmpty() || userName.Length > 100)
        {
            throw new DomainValidateNotificationException("Логин владельца должен быть заполнен");
        }

        if (externalId.IsNullOrEmpty() || externalId.Length > 200)
        {
            throw new DomainValidateNotificationException("Внешний идентификатор получателя должен быть заполнен");
        }

        return new NotificationRecipient
        {
            UserName = userName,
            Channel = channel,
            ExternalId = externalId,
            DisplayName = displayName
        };
    }

    public void UpdateDisplayName(string? displayName)
    {
        DisplayName = displayName;
    }

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;

    public void Delete(DateTime dateDeleted) => DateDeleted = dateDeleted;
}
