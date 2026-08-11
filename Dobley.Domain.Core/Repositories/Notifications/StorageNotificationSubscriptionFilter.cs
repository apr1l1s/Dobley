namespace Dobley.Domain.Core.Repositories.Notifications;

public class StorageNotificationSubscriptionFilter(params int[] ids)
{
    public IReadOnlyList<int>? Ids { get; set; } = ids;
}
