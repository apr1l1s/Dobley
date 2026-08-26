using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingNotificationDeliveryTestCase(string TestName, string UserName, NotificationChannel Channel,
    string Destination, int ProductId, DateTime ExpirationDate, string Subject, string Body, bool IsValid,
    int? ExpectedProductId = null)
{
    public override string ToString() => TestName;
}

public class CreatingNotificationDeliveryTestDataGenerator
    : DataGenerator<CreatingNotificationDeliveryTestCase>
{
    protected override IEnumerable<CreatingNotificationDeliveryTestCase> GetData()
    {
        yield return new CreatingNotificationDeliveryTestCase(
            TestName: "4.1 Корректный факт отправки уведомления",
            UserName: "demo",
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            ProductId: 2,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Subject: "Срок годности продукта",
            Body: "Текст уведомления",
            IsValid: true,
            ExpectedProductId: 2);

        yield return new CreatingNotificationDeliveryTestCase(
            TestName: "4.2 Некорректный пользователь уведомления",
            UserName: "",
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            ProductId: 2,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Subject: "Срок годности продукта",
            Body: "Текст уведомления",
            IsValid: false);

        yield return new CreatingNotificationDeliveryTestCase(
            TestName: "4.3 Некорректный продукт уведомления",
            UserName: "demo",
            Channel: NotificationChannel.Telegram,
            Destination: "123456",
            ProductId: 0,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Subject: "Срок годности продукта",
            Body: "Текст уведомления",
            IsValid: false);

        yield return new CreatingNotificationDeliveryTestCase(
            TestName: "4.4 Некорректный адрес доставки уведомления",
            UserName: "demo",
            Channel: NotificationChannel.Telegram,
            Destination: "",
            ProductId: 2,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Subject: "Срок годности продукта",
            Body: "Текст уведомления",
            IsValid: false);
    }
}
