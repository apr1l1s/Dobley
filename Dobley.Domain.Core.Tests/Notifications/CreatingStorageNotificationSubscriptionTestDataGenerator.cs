namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingStorageNotificationSubscriptionTestCase(string TestName, int NotificationRecipientId,
    int StorageId, int NotifyBeforeDays, bool IsValid, int? ExpectedRecipientId = null,
    int? ExpectedStorageId = null, int? ExpectedNotifyBeforeDays = null)
{
    public override string ToString() => TestName;
}

public class CreatingStorageNotificationSubscriptionTestDataGenerator
    : DataGenerator<CreatingStorageNotificationSubscriptionTestCase>
{
    protected override IEnumerable<CreatingStorageNotificationSubscriptionTestCase> GetData()
    {
        yield return new CreatingStorageNotificationSubscriptionTestCase(
            TestName: "3.1 Корректная подписка на хранилище",
            NotificationRecipientId: 1,
            StorageId: 1,
            NotifyBeforeDays: 3,
            IsValid: true,
            ExpectedRecipientId: 1,
            ExpectedStorageId: 1,
            ExpectedNotifyBeforeDays: 3);

        yield return new CreatingStorageNotificationSubscriptionTestCase(
            TestName: "3.2 Некорректный получатель подписки",
            NotificationRecipientId: 0,
            StorageId: 1,
            NotifyBeforeDays: 3,
            IsValid: false);

        yield return new CreatingStorageNotificationSubscriptionTestCase(
            TestName: "3.3 Некорректное хранилище подписки",
            NotificationRecipientId: 1,
            StorageId: 0,
            NotifyBeforeDays: 3,
            IsValid: false);

        yield return new CreatingStorageNotificationSubscriptionTestCase(
            TestName: "3.4 Некорректное количество дней до уведомления",
            NotificationRecipientId: 1,
            StorageId: 1,
            NotifyBeforeDays: 31,
            IsValid: false);
    }
}
