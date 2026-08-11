using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Endpoints.Api.Dto;

public record NotificationRecipientResponse(int Id, string Channel, string ExternalId, string? DisplayName)
{
    public static NotificationRecipientResponse Create(NotificationRecipient recipient)
        => new(recipient.Id, recipient.Channel.ToString(), recipient.ExternalId, recipient.DisplayName);
}
