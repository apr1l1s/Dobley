using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Notifications;

public class NotificationInboxMessageRepository(DobleyContext context)
    : RepositoryBase<NotificationInboxMessage, NotificationInboxMessageFilter>(context),
        INotificationInboxMessageRepository
{
    public override async Task<IReadOnlyList<NotificationInboxMessage>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new NotificationInboxMessageFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<NotificationInboxMessage>?> GetCollectionAsync(
        NotificationInboxMessageFilter filter, CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default)
        => Context.NotificationInboxMessages.AnyAsync(x => x.MessageId == messageId, cancellationToken);

    public override Task<PaginatedCollection<NotificationInboxMessage>> GetPaginatedCollection(
        NotificationInboxMessageFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    private IQueryable<NotificationInboxMessage> FilterEntities(NotificationInboxMessageFilter? filter)
    {
        var messages = Context.NotificationInboxMessages.AsQueryable();

        if (filter?.Ids is { Count: > 0 })
        {
            messages = messages.Where(x => filter.Ids.Contains(x.Id));
        }

        return messages;
    }
}
