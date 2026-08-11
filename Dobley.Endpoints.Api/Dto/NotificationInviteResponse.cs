using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Endpoints.Api.Dto;

public record NotificationInviteResponse(int Id, string Code, DateTime ExpiresAt)
{
    public static NotificationInviteResponse Create(NotificationInvite invite)
        => new(invite.Id, invite.Code, invite.ExpiresAt);
}
