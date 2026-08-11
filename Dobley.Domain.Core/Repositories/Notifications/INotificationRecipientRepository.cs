using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface INotificationRecipientRepository
    : IRepository<NotificationRecipient, NotificationRecipientFilter>
{
    Task<NotificationRecipient?> GetByChannelAndExternalIdAsync(NotificationChannel channel, string externalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationRecipient>> GetCollectionForUserAsync(string userName,
        CancellationToken cancellationToken = default);

    Task<NotificationRecipient?> GetForUserAsync(int id, string userName,
        CancellationToken cancellationToken = default);
}
