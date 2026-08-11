using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Notifications;

public class NotificationRecipientRepository(DobleyContext context)
    : RepositoryBase<NotificationRecipient, NotificationRecipientFilter>(context), INotificationRecipientRepository
{
    public override async Task<IReadOnlyList<NotificationRecipient>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new NotificationRecipientFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<NotificationRecipient>?> GetCollectionAsync(
        NotificationRecipientFilter filter, CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NotificationRecipient>> GetCollectionByChannelAndExternalIdAsync(
        NotificationChannel channel, string externalId, CancellationToken cancellationToken = default)
        => await Context.NotificationRecipients
            .Where(x => x.Channel == channel && x.ExternalId == externalId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<NotificationRecipient?> GetForUserAsync(string userName, NotificationChannel channel, string externalId,
        CancellationToken cancellationToken = default)
        => Context.NotificationRecipients.FirstOrDefaultAsync(
            x => x.UserName == userName && x.Channel == channel && x.ExternalId == externalId, cancellationToken);

    public async Task<IReadOnlyList<NotificationRecipient>> GetCollectionForUserAsync(string userName,
        CancellationToken cancellationToken = default)
        => await Context.NotificationRecipients
            .Where(x => x.UserName == userName)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<NotificationRecipient?> GetForUserAsync(int id, string userName,
        CancellationToken cancellationToken = default)
        => Context.NotificationRecipients.FirstOrDefaultAsync(x => x.Id == id && x.UserName == userName,
            cancellationToken);

    public override Task<PaginatedCollection<NotificationRecipient>> GetPaginatedCollection(
        NotificationRecipientFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    private IQueryable<NotificationRecipient> FilterEntities(NotificationRecipientFilter? filter)
    {
        var recipients = Context.NotificationRecipients.AsQueryable();

        if (filter?.Ids is { Count: > 0 })
        {
            recipients = recipients.Where(x => filter.Ids.Contains(x.Id));
        }

        return recipients;
    }
}
