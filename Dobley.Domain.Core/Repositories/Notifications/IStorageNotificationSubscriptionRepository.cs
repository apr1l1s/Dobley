using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Repositories.Notifications;

public interface IStorageNotificationSubscriptionRepository
    : IRepository<StorageNotificationSubscription, StorageNotificationSubscriptionFilter>
{
    Task AddRangeAsync(IReadOnlyCollection<StorageNotificationSubscription> subscriptions,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StorageNotificationSubscription>> GetEnabledSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StorageNotificationSubscription>> GetForRecipientAsync(int notificationRecipientId,
        IReadOnlyCollection<int> storageIds, CancellationToken cancellationToken = default);

    Task<StorageNotificationSubscription?> GetForRecipientAndStorageAsync(int notificationRecipientId, int storageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetStorageIdsAsync(int notificationRecipientId,
        CancellationToken cancellationToken = default);
}
