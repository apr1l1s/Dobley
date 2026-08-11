using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Notifications;

public class UpdatingStorageNotificationSubscriptionTests
{
    [Fact]
    public void Disable_MarksSubscriptionAsDisabled()
    {
        var subscription = StorageNotificationSubscription.Create(1, 1, 3);

        subscription.Disable();

        Assert.False(subscription.IsEnabled);
    }

    [Fact]
    public void Enable_MarksSubscriptionAsEnabled()
    {
        var subscription = StorageNotificationSubscription.Create(1, 1, 3);
        subscription.Disable();

        subscription.Enable();

        Assert.True(subscription.IsEnabled);
    }
}
