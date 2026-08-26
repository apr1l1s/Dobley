using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Notifications;

public class CreatingNotificationInboxMessageTests
{
    [Theory]
    [ClassData(typeof(CreatingNotificationInboxMessageTestDataGenerator))]
    public void Create_ReturnsInboxMessageOrThrowsDomainException(CreatingNotificationInboxMessageTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateNotificationException>(() => CreateInboxMessage(testCase));
            return;
        }

        var message = CreateInboxMessage(testCase);

        Assert.Equal(testCase.MessageId, message.MessageId);
        Assert.Equal(testCase.Channel, message.Channel);
        Assert.Equal(testCase.Destination, message.Destination);
    }

    private static NotificationInboxMessage CreateInboxMessage(CreatingNotificationInboxMessageTestCase testCase)
        => NotificationInboxMessage.Create(testCase.MessageId, testCase.Channel, testCase.Destination);
}
