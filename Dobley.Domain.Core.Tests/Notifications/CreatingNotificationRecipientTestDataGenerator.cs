using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingNotificationRecipientTestCase(string TestName, string? UserName, NotificationChannel Channel,
    string? ExternalId, string? DisplayName, bool IsValid, string? ExpectedUserName = null,
    string? ExpectedExternalId = null, NotificationChannel? ExpectedChannel = null)
{
    public override string ToString() => TestName;
}

public class CreatingNotificationRecipientTestDataGenerator
    : DataGenerator<CreatingNotificationRecipientTestCase>
{
    protected override IEnumerable<CreatingNotificationRecipientTestCase> GetData()
    {
        yield return new CreatingNotificationRecipientTestCase(
            TestName: "2.1 Корректный получатель уведомлений",
            UserName: "demo",
            Channel: NotificationChannel.Telegram,
            ExternalId: "123456",
            DisplayName: "Demo",
            IsValid: true,
            ExpectedUserName: "demo",
            ExpectedExternalId: "123456",
            ExpectedChannel: NotificationChannel.Telegram);

        yield return new CreatingNotificationRecipientTestCase(
            TestName: "2.2 Некорректный владелец получателя",
            UserName: null,
            Channel: NotificationChannel.Telegram,
            ExternalId: "123456",
            DisplayName: "Demo",
            IsValid: false);

        yield return new CreatingNotificationRecipientTestCase(
            TestName: "2.3 Некорректный внешний идентификатор",
            UserName: "demo",
            Channel: NotificationChannel.Telegram,
            ExternalId: null,
            DisplayName: "Demo",
            IsValid: false);
    }
}
