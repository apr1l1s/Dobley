using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeNotificationOutboxMessageRepository(params NotificationOutboxMessage[] messages)
    : INotificationOutboxMessageRepository
{
    private readonly List<NotificationOutboxMessage> _messages = [..messages];

    public IReadOnlyList<NotificationOutboxMessage> AddedMessages => _messages;

    public Task<NotificationOutboxMessage> AddAsync(NotificationOutboxMessage entity,
        CancellationToken cancellationToken = default)
    {
        if (entity.Id == 0)
        {
            entity.Id = _messages.Count == 0 ? 1 : _messages.Max(x => x.Id) + 1;
        }

        _messages.Add(entity);
        return Task.FromResult(entity);
    }

    public void Delete(NotificationOutboxMessage entity)
    {
    }

    public Task<IReadOnlyList<NotificationOutboxMessage>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => Task.FromResult<IReadOnlyList<NotificationOutboxMessage>>(_messages
            .Where(x => ids.Contains(x.Id))
            .ToArray());

    public Task<IReadOnlyList<NotificationOutboxMessage>?> GetCollectionAsync(NotificationOutboxMessageFilter filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationOutboxMessage>?>(_messages.ToArray());

    public Task<NotificationOutboxMessage> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_messages.Single(x => x.Id == id));

    public Task<NotificationOutboxMessage?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_messages.SingleOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<NotificationOutboxMessage>> GetPendingAsync(int count,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationOutboxMessage>>(_messages
            .Where(x => x.DateProcessed == null)
            .Take(count)
            .ToArray());

    public Task<PaginatedCollection<NotificationOutboxMessage>> GetPaginatedCollection(
        NotificationOutboxMessageFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new PaginatedCollection<NotificationOutboxMessage>(_messages, pageNumber, pageSize,
            _messages.Count));
}
