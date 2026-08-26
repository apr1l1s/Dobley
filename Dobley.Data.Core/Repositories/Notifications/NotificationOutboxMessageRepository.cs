using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Notifications;

public class NotificationOutboxMessageRepository(DobleyContext context)
    : RepositoryBase<NotificationOutboxMessage, NotificationOutboxMessageFilter>(context),
        INotificationOutboxMessageRepository
{
    public override async Task<IReadOnlyList<NotificationOutboxMessage>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new NotificationOutboxMessageFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<NotificationOutboxMessage>?> GetCollectionAsync(
        NotificationOutboxMessageFilter filter, CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NotificationOutboxMessage>> GetPendingAsync(int count,
        CancellationToken cancellationToken = default)
        => await Context.NotificationOutboxMessages
            .Where(x => x.DateProcessed == null)
            .OrderBy(x => x.DateAdded)
            .ThenBy(x => x.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

    public override Task<PaginatedCollection<NotificationOutboxMessage>> GetPaginatedCollection(
        NotificationOutboxMessageFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    private IQueryable<NotificationOutboxMessage> FilterEntities(NotificationOutboxMessageFilter? filter)
    {
        var messages = Context.NotificationOutboxMessages.AsQueryable();

        if (filter?.Ids is { Count: > 0 })
        {
            messages = messages.Where(x => filter.Ids.Contains(x.Id));
        }

        return messages;
    }
}
