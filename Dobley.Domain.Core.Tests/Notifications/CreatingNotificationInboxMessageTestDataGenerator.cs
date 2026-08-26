using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingNotificationInboxMessageTestCase(string TestName, Guid MessageId, NotificationChannel Channel,
    string Destination, bool IsValid)
{
    public override string ToString() => TestName;
}

public class CreatingNotificationInboxMessageTestDataGenerator
    : DataGenerator<CreatingNotificationInboxMessageTestCase>
{
    protected override IEnumerable<CreatingNotificationInboxMessageTestCase> GetData()
    {
        yield return new CreatingNotificationInboxMessageTestCase(
            TestName: "5.1 Корректное входящее уведомление",
            MessageId: Guid.NewGuid(),
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            IsValid: true);

        yield return new CreatingNotificationInboxMessageTestCase(
            TestName: "5.2 Некорректный идентификатор входящего уведомления",
            MessageId: Guid.Empty,
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            IsValid: false);

        yield return new CreatingNotificationInboxMessageTestCase(
            TestName: "5.3 Некорректный адрес входящего уведомления",
            MessageId: Guid.NewGuid(),
            Channel: NotificationChannel.Telegram,
            Destination: "",
            IsValid: false);
    }
}
