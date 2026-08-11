using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Notifications;

public class NotificationInvite
    : IAuditableEntity, ISoftDeletedEntity
{
    public int Id { get; set; }

    public string UserName { get; private set; } = null!;

    public string Code { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public User? DomainUser { get; private set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    public DateTime? DateDeleted { get; private set; }

    public bool IsDeleted => DateDeleted.HasValue;

    public bool IsUsed => UsedAt.HasValue;

    private NotificationInvite()
    {
    }

    public static NotificationInvite Create(string userName, string code, DateTime expiresAt)
    {
        if (userName.IsNullOrEmpty() || userName.Length > 100)
        {
            throw new DomainValidateNotificationException("Логин владельца должен быть заполнен");
        }

        if (code.IsNullOrEmpty() || code.Length > 100)
        {
            throw new DomainValidateNotificationException("Код приглашения должен быть заполнен");
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new DomainValidateNotificationException("Срок действия приглашения должен быть в будущем");
        }

        return new NotificationInvite
        {
            UserName = userName,
            Code = code,
            ExpiresAt = expiresAt
        };
    }

    public bool CanBeUsed(DateTime now) => !IsDeleted && !IsUsed && ExpiresAt > now;

    public void MarkUsed(DateTime usedAt) => UsedAt = usedAt;

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;

    public void Delete(DateTime dateDeleted) => DateDeleted = dateDeleted;
}
