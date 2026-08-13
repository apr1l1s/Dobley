using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeStorageNotificationSubscriptionRepository(params StorageNotificationSubscription[] subscriptions)
    : IStorageNotificationSubscriptionRepository
{
    private readonly List<StorageNotificationSubscription> _subscriptions = [..subscriptions];

    public IReadOnlyList<StorageNotificationSubscription> AddedSubscriptions => _subscriptions;

    public Task<StorageNotificationSubscription> AddAsync(StorageNotificationSubscription entity,
        CancellationToken cancellationToken = default)
    {
        _subscriptions.Add(entity);
        return Task.FromResult(entity);
    }

    public Task AddRangeAsync(IReadOnlyCollection<StorageNotificationSubscription> subscriptions,
        CancellationToken cancellationToken = default)
    {
        _subscriptions.AddRange(subscriptions);
        return Task.CompletedTask;
    }

    public void Delete(StorageNotificationSubscription entity)
    {
    }

    public Task<IReadOnlyList<StorageNotificationSubscription>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => Task.FromResult<IReadOnlyList<StorageNotificationSubscription>>(_subscriptions
            .Where(x => ids.Contains(x.Id))
            .ToArray());

    public Task<IReadOnlyList<StorageNotificationSubscription>?> GetCollectionAsync(
        StorageNotificationSubscriptionFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StorageNotificationSubscription>?>(_subscriptions.ToArray());

    public Task<IReadOnlyList<StorageNotificationSubscription>> GetEnabledSubscriptionsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StorageNotificationSubscription>>(_subscriptions
            .Where(x => x.IsEnabled)
            .ToArray());

    public Task<IReadOnlyList<StorageNotificationSubscription>> GetForRecipientAsync(int notificationRecipientId,
        IReadOnlyCollection<int> storageIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StorageNotificationSubscription>>(_subscriptions
            .Where(x => x.NotificationRecipientId == notificationRecipientId && storageIds.Contains(x.StorageId))
            .ToArray());

    public Task<IReadOnlyList<StorageNotificationSubscription>> GetForRecipientAsync(int notificationRecipientId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StorageNotificationSubscription>>(_subscriptions
            .Where(x => x.NotificationRecipientId == notificationRecipientId)
            .ToArray());

    public Task<IReadOnlyList<int>> GetStorageIdsAsync(int notificationRecipientId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<int>>(_subscriptions
            .Where(x => x.NotificationRecipientId == notificationRecipientId)
            .Select(x => x.StorageId)
            .ToArray());

    public Task<StorageNotificationSubscription> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_subscriptions.Single(x => x.Id == id));

    public Task<StorageNotificationSubscription?> GetItemNullable(int id,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_subscriptions.SingleOrDefault(x => x.Id == id));

    public Task<PaginatedCollection<StorageNotificationSubscription>> GetPaginatedCollection(
        StorageNotificationSubscriptionFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new PaginatedCollection<StorageNotificationSubscription>(_subscriptions, pageNumber,
            pageSize, _subscriptions.Count));
}
