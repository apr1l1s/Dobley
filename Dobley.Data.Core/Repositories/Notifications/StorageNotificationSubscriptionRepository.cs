using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Notifications;

public class StorageNotificationSubscriptionRepository(DobleyContext context)
    : RepositoryBase<StorageNotificationSubscription, StorageNotificationSubscriptionFilter>(context),
        IStorageNotificationSubscriptionRepository
{
    public Task AddRangeAsync(IReadOnlyCollection<StorageNotificationSubscription> subscriptions,
        CancellationToken cancellationToken = default)
        => Context.StorageNotificationSubscriptions.AddRangeAsync(subscriptions, cancellationToken);

    public override async Task<IReadOnlyList<StorageNotificationSubscription>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new StorageNotificationSubscriptionFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<StorageNotificationSubscription>?> GetCollectionAsync(
        StorageNotificationSubscriptionFilter filter, CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StorageNotificationSubscription>> GetEnabledSubscriptionsAsync(
        CancellationToken cancellationToken = default)
        => await Context.StorageNotificationSubscriptions
            .Include(x => x.DomainNotificationRecipient)
            .Include(x => x.DomainStorage)
            .Where(x => x.IsEnabled)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StorageNotificationSubscription>> GetForRecipientAsync(int notificationRecipientId,
        IReadOnlyCollection<int> storageIds, CancellationToken cancellationToken = default)
        => await Context.StorageNotificationSubscriptions
            .Where(x => x.NotificationRecipientId == notificationRecipientId && storageIds.Contains(x.StorageId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StorageNotificationSubscription>> GetForRecipientAsync(int notificationRecipientId,
        CancellationToken cancellationToken = default)
        => await Context.StorageNotificationSubscriptions
            .Where(x => x.NotificationRecipientId == notificationRecipientId)
            .ToListAsync(cancellationToken);

    public override Task<PaginatedCollection<StorageNotificationSubscription>> GetPaginatedCollection(
        StorageNotificationSubscriptionFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    public async Task<IReadOnlyList<int>> GetStorageIdsAsync(int notificationRecipientId,
        CancellationToken cancellationToken = default)
        => await Context.StorageNotificationSubscriptions
            .Where(x => x.NotificationRecipientId == notificationRecipientId)
            .Select(x => x.StorageId)
            .ToListAsync(cancellationToken);

    private IQueryable<StorageNotificationSubscription> FilterEntities(StorageNotificationSubscriptionFilter? filter)
    {
        var subscriptions = Context.StorageNotificationSubscriptions.AsQueryable();

        if (filter?.Ids is { Count: > 0 })
        {
            subscriptions = subscriptions.Where(x => filter.Ids.Contains(x.Id));
        }

        return subscriptions;
    }
}
