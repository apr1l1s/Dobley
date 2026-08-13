namespace Dobley.Domain.Core.Repositories.Notifications;

public class NotificationDeliveryFilter(params int[] ids)
{
    public IReadOnlyList<int>? Ids { get; set; } = ids;
}
