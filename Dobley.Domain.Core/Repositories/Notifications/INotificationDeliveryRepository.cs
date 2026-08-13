using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface INotificationDeliveryRepository
    : IRepository<NotificationDelivery, NotificationDeliveryFilter>
{
    Task<bool> ExistsAsync(int notificationRecipientId, int productId, DateTime expirationDate,
        NotificationChannel channel, CancellationToken cancellationToken = default);
}
