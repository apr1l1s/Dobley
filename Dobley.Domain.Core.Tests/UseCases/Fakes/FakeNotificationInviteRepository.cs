using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeNotificationInviteRepository(params NotificationInvite[] invites) : INotificationInviteRepository
{
    private readonly List<NotificationInvite> _invites = [..invites];

    public IReadOnlyList<NotificationInvite> AddedInvites => _invites;

    public Task<NotificationInvite> AddAsync(NotificationInvite entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id == 0)
        {
            entity.Id = _invites.Count == 0 ? 1 : _invites.Max(x => x.Id) + 1;
        }

        _invites.Add(entity);
        return Task.FromResult(entity);
    }

    public void Delete(NotificationInvite entity)
    {
    }

    public Task<NotificationInvite?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult(_invites.SingleOrDefault(x => x.Code == code));

    public Task<IReadOnlyList<NotificationInvite>> GetCollectionAsync(CancellationToken cancellationToken = default,
        params int[] ids)
        => Task.FromResult<IReadOnlyList<NotificationInvite>>(_invites.Where(x => ids.Contains(x.Id)).ToArray());

    public Task<IReadOnlyList<NotificationInvite>?> GetCollectionAsync(NotificationInviteFilter filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationInvite>?>(_invites.ToArray());

    public Task<NotificationInvite> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_invites.Single(x => x.Id == id));

    public Task<NotificationInvite?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_invites.SingleOrDefault(x => x.Id == id));

    public Task<PaginatedCollection<NotificationInvite>> GetPaginatedCollection(NotificationInviteFilter? filter,
        int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaginatedCollection<NotificationInvite>(_invites, pageNumber, pageSize,
            _invites.Count));
}
