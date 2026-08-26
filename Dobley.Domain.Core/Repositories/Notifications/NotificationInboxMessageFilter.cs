namespace Dobley.Domain.Core.Repositories.Notifications;

public class NotificationInboxMessageFilter(params int[] ids)
{
    public IReadOnlyList<int> Ids { get; private set; } = ids;
}
