namespace Dobley.Domain.Core.Repositories.Notifications;

public class NotificationInviteFilter(params int[] ids)
{
    public IReadOnlyList<int>? Ids { get; set; } = ids;
}
