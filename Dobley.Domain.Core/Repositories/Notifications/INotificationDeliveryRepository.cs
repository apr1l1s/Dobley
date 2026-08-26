using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface INotificationDeliveryRepository
    : IRepository<NotificationDelivery, NotificationDeliveryFilter>
{
    Task<bool> ExistsAsync(string userName, NotificationChannel channel, string destination, int productId,
        DateTime expirationDate, CancellationToken cancellationToken = default);
}
