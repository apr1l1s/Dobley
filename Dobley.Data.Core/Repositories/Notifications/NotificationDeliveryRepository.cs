using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Notifications;

public class NotificationDeliveryRepository(DobleyContext context)
    : RepositoryBase<NotificationDelivery, NotificationDeliveryFilter>(context), INotificationDeliveryRepository
{
    public override async Task<IReadOnlyList<NotificationDelivery>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new NotificationDeliveryFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<NotificationDelivery>?> GetCollectionAsync(
        NotificationDeliveryFilter filter, CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int notificationRecipientId, int productId, DateTime expirationDate,
        NotificationChannel channel, CancellationToken cancellationToken = default)
        => Context.NotificationDeliveries.AnyAsync(x =>
            x.NotificationRecipientId == notificationRecipientId &&
            x.ProductId == productId &&
            x.ExpirationDate == expirationDate.Date &&
            x.Channel == channel, cancellationToken);

    public override Task<PaginatedCollection<NotificationDelivery>> GetPaginatedCollection(
        NotificationDeliveryFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    private IQueryable<NotificationDelivery> FilterEntities(NotificationDeliveryFilter? filter)
    {
        var deliveries = Context.NotificationDeliveries.AsQueryable();

        if (filter?.Ids is { Count: > 0 })
        {
            deliveries = deliveries.Where(x => filter.Ids.Contains(x.Id));
        }

        return deliveries;
    }
}
