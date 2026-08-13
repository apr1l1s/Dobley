using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeNotificationRecipientRepository(params NotificationRecipient[] recipients)
    : INotificationRecipientRepository
{
    private readonly List<NotificationRecipient> _recipients = [..recipients];

    public Task<NotificationRecipient> AddAsync(NotificationRecipient entity,
        CancellationToken cancellationToken = default)
    {
        if (entity.Id == 0)
        {
            entity.Id = _recipients.Count == 0 ? 1 : _recipients.Max(x => x.Id) + 1;
        }

        _recipients.Add(entity);
        return Task.FromResult(entity);
    }

    public void Delete(NotificationRecipient entity)
    {
    }

    public Task<IReadOnlyList<NotificationRecipient>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => Task.FromResult<IReadOnlyList<NotificationRecipient>>(_recipients
            .Where(x => ids.Contains(x.Id))
            .ToArray());

    public Task<IReadOnlyList<NotificationRecipient>?> GetCollectionAsync(NotificationRecipientFilter filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationRecipient>?>(_recipients.ToArray());

    public Task<IReadOnlyList<NotificationRecipient>> GetCollectionByChannelAndExternalIdAsync(
        NotificationChannel channel, string externalId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationRecipient>>(_recipients
            .Where(x => x.Channel == channel && x.ExternalId == externalId)
            .ToArray());

    public Task<IReadOnlyList<NotificationRecipient>> GetCollectionForUserAsync(string userName,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationRecipient>>(_recipients
            .Where(x => x.UserName == userName)
            .ToArray());

    public Task<NotificationRecipient?> GetForUserAsync(string userName, NotificationChannel channel,
        string externalId, CancellationToken cancellationToken = default)
        => Task.FromResult(_recipients.SingleOrDefault(x =>
            x.UserName == userName && x.Channel == channel && x.ExternalId == externalId));

    public Task<NotificationRecipient?> GetForUserAsync(int id, string userName,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_recipients.SingleOrDefault(x => x.Id == id && x.UserName == userName));

    public Task<NotificationRecipient> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_recipients.Single(x => x.Id == id));

    public Task<NotificationRecipient?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_recipients.SingleOrDefault(x => x.Id == id));

    public Task<PaginatedCollection<NotificationRecipient>> GetPaginatedCollection(NotificationRecipientFilter? filter,
        int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaginatedCollection<NotificationRecipient>(_recipients, pageNumber, pageSize,
            _recipients.Count));
}
