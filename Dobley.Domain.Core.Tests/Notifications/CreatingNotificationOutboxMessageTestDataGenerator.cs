using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingNotificationOutboxMessageTestCase(string TestName, NotificationChannel Channel,
    string Destination, string Subject, string Body, bool IsValid)
{
    public override string ToString() => TestName;
}

public class CreatingNotificationOutboxMessageTestDataGenerator
    : DataGenerator<CreatingNotificationOutboxMessageTestCase>
{
    protected override IEnumerable<CreatingNotificationOutboxMessageTestCase> GetData()
    {
        yield return new CreatingNotificationOutboxMessageTestCase(
            TestName: "6.1 Корректное исходящее уведомление",
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            Subject: "Срок годности продукта",
            Body: "Братан, у продукта \"Хлеб\" скоро закончится срок годности.",
            IsValid: true);

        yield return new CreatingNotificationOutboxMessageTestCase(
            TestName: "6.2 Некорректный адрес исходящего уведомления",
            Channel: NotificationChannel.Telegram,
            Destination: "",
            Subject: "Срок годности продукта",
            Body: "Текст уведомления",
            IsValid: false);

        yield return new CreatingNotificationOutboxMessageTestCase(
            TestName: "6.3 Некорректный заголовок исходящего уведомления",
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            Subject: "",
            Body: "Текст уведомления",
            IsValid: false);

        yield return new CreatingNotificationOutboxMessageTestCase(
            TestName: "6.4 Некорректный текст исходящего уведомления",
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            Subject: "Срок годности продукта",
            Body: "",
            IsValid: false);
    }
}
