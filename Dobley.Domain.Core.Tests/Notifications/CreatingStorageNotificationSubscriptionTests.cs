using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Notifications;

public class CreatingStorageNotificationSubscriptionTests
{
    [Theory]
    [ClassData(typeof(CreatingStorageNotificationSubscriptionTestDataGenerator))]
    public void Create_ReturnsSubscriptionOrThrowsDomainException(CreatingStorageNotificationSubscriptionTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateNotificationException>(() => CreateSubscription(testCase));
            return;
        }

        var subscription = CreateSubscription(testCase);

        Assert.Equal(testCase.ExpectedRecipientId, subscription.NotificationRecipientId);
        Assert.Equal(testCase.ExpectedStorageId, subscription.StorageId);
        Assert.Equal(testCase.ExpectedNotifyBeforeDays, subscription.NotifyBeforeDays);
        Assert.True(subscription.IsEnabled);
    }

    private static StorageNotificationSubscription CreateSubscription(
        CreatingStorageNotificationSubscriptionTestCase testCase)
        => StorageNotificationSubscription.Create(testCase.NotificationRecipientId, testCase.StorageId,
            testCase.NotifyBeforeDays);
}
