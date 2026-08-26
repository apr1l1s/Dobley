using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface INotificationOutboxMessageRepository
    : IRepository<NotificationOutboxMessage, NotificationOutboxMessageFilter>
{
    Task<IReadOnlyList<NotificationOutboxMessage>> GetPendingAsync(int count,
        CancellationToken cancellationToken = default);
}
