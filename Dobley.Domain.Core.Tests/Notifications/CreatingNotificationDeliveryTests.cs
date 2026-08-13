using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Notifications;

public class CreatingNotificationDeliveryTests
{
    [Theory]
    [ClassData(typeof(CreatingNotificationDeliveryTestDataGenerator))]
    public void Create_ReturnsDeliveryOrThrowsDomainException(CreatingNotificationDeliveryTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateNotificationException>(() => CreateDelivery(testCase));
            return;
        }

        var delivery = CreateDelivery(testCase);

        Assert.Equal(testCase.ExpectedRecipientId, delivery.NotificationRecipientId);
        Assert.Equal(testCase.ExpectedProductId, delivery.ProductId);
        Assert.Equal(testCase.ExpirationDate.Date, delivery.ExpirationDate);
        Assert.Equal(testCase.Channel, delivery.Channel);
    }

    private static NotificationDelivery CreateDelivery(CreatingNotificationDeliveryTestCase testCase)
        => NotificationDelivery.Create(testCase.NotificationRecipientId, testCase.ProductId,
            testCase.ExpirationDate, testCase.Channel);
}
