using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingNotificationDeliveryTestCase(string TestName, int NotificationRecipientId, int ProductId,
    DateTime ExpirationDate, NotificationChannel Channel, bool IsValid, int? ExpectedRecipientId = null,
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
            NotificationRecipientId: 1,
            ProductId: 2,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Channel: NotificationChannel.Telegram,
            IsValid: true,
            ExpectedRecipientId: 1,
            ExpectedProductId: 2);

        yield return new CreatingNotificationDeliveryTestCase(
            TestName: "4.2 Некорректный получатель уведомления",
            NotificationRecipientId: 0,
            ProductId: 2,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Channel: NotificationChannel.Telegram,
            IsValid: false);

        yield return new CreatingNotificationDeliveryTestCase(
            TestName: "4.3 Некорректный продукт уведомления",
            NotificationRecipientId: 1,
            ProductId: 0,
            ExpirationDate: DateTime.UtcNow.AddDays(3),
            Channel: NotificationChannel.Telegram,
            IsValid: false);
    }
}
