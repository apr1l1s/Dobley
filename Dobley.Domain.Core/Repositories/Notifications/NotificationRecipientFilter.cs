namespace Dobley.Domain.Core.Repositories.Notifications;

public class NotificationRecipientFilter(params int[] ids)
{
    public IReadOnlyList<int>? Ids { get; set; } = ids;
}
