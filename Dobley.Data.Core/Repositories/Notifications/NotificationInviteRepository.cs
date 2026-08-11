using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Notifications;

public class NotificationInviteRepository(DobleyContext context)
    : RepositoryBase<NotificationInvite, NotificationInviteFilter>(context), INotificationInviteRepository
{
    public override async Task<IReadOnlyList<NotificationInvite>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new NotificationInviteFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<NotificationInvite>?> GetCollectionAsync(NotificationInviteFilter filter,
        CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public Task<NotificationInvite?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => Context.NotificationInvites.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public override Task<PaginatedCollection<NotificationInvite>> GetPaginatedCollection(
        NotificationInviteFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    private IQueryable<NotificationInvite> FilterEntities(NotificationInviteFilter? filter)
    {
        var invites = Context.NotificationInvites.AsQueryable();

        if (filter?.Ids is { Count: > 0 })
        {
            invites = invites.Where(x => filter.Ids.Contains(x.Id));
        }

        return invites;
    }
}
