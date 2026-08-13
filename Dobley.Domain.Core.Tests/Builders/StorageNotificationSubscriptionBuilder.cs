using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Builders;

public static class StorageNotificationSubscriptionBuilder
{
    public static int LastId { get; private set; }

    public static StorageNotificationSubscription Build(int? id = null, int notificationRecipientId = 1,
        int storageId = 1, int notifyBeforeDays = 3)
    {
        var subscription = StorageNotificationSubscription.Create(notificationRecipientId, storageId,
            notifyBeforeDays);
        subscription.Id = id ?? ++LastId;

        return subscription;
    }
}
