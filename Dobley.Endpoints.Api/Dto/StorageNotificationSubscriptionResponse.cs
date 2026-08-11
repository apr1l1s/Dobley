using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Endpoints.Api.Dto;

public record StorageNotificationSubscriptionResponse(int Id, int RecipientId, int StorageId, int NotifyBeforeDays,
    bool IsEnabled)
{
    public static StorageNotificationSubscriptionResponse Create(StorageNotificationSubscription subscription)
        => new(subscription.Id, subscription.NotificationRecipientId, subscription.StorageId,
            subscription.NotifyBeforeDays, subscription.IsEnabled);
}
