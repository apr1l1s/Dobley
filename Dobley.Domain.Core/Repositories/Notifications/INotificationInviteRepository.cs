using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface INotificationInviteRepository : IRepository<NotificationInvite, NotificationInviteFilter>
{
    Task<NotificationInvite?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
