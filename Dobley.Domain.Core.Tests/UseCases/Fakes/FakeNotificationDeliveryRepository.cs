using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeNotificationDeliveryRepository(params NotificationDelivery[] deliveries)
    : INotificationDeliveryRepository
{
    private readonly List<NotificationDelivery> _deliveries = [..deliveries];

    public IReadOnlyList<NotificationDelivery> AddedDeliveries => _deliveries;

    public Task<NotificationDelivery> AddAsync(NotificationDelivery entity,
        CancellationToken cancellationToken = default)
    {
        if (entity.Id == 0)
        {
            entity.Id = _deliveries.Count == 0 ? 1 : _deliveries.Max(x => x.Id) + 1;
        }

        _deliveries.Add(entity);
        return Task.FromResult(entity);
    }

    public void Delete(NotificationDelivery entity)
    {
    }

    public Task<bool> ExistsAsync(string userName, NotificationChannel channel, string destination, int productId,
        DateTime expirationDate, CancellationToken cancellationToken = default)
        => Task.FromResult(_deliveries.Any(x =>
            x.UserName == userName &&
            x.Channel == channel &&
            x.Destination == destination &&
            x.ProductId == productId &&
            x.ExpirationDate == expirationDate.Date));

    public Task<IReadOnlyList<NotificationDelivery>> GetCollectionAsync(CancellationToken cancellationToken = default,
        params int[] ids)
        => Task.FromResult<IReadOnlyList<NotificationDelivery>>(_deliveries
            .Where(x => ids.Contains(x.Id))
            .ToArray());

    public Task<IReadOnlyList<NotificationDelivery>?> GetCollectionAsync(NotificationDeliveryFilter filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationDelivery>?>(_deliveries.ToArray());

    public Task<NotificationDelivery> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_deliveries.Single(x => x.Id == id));

    public Task<NotificationDelivery?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_deliveries.SingleOrDefault(x => x.Id == id));

    public Task<PaginatedCollection<NotificationDelivery>> GetPaginatedCollection(NotificationDeliveryFilter? filter,
        int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaginatedCollection<NotificationDelivery>(_deliveries, pageNumber, pageSize,
            _deliveries.Count));
}
