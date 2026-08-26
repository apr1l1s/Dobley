using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface INotificationInboxMessageRepository
    : IRepository<NotificationInboxMessage, NotificationInboxMessageFilter>
{
    Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default);
}
