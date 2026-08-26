using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Notifications;

public class CreatingNotificationOutboxMessageTests
{
    [Theory]
    [ClassData(typeof(CreatingNotificationOutboxMessageTestDataGenerator))]
    public void Create_ReturnsOutboxMessageOrThrowsDomainException(CreatingNotificationOutboxMessageTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateNotificationException>(() => CreateOutboxMessage(testCase));
            return;
        }

        var message = CreateOutboxMessage(testCase);

        Assert.NotEqual(Guid.Empty, message.MessageId);
        Assert.Equal(testCase.Channel, message.Channel);
        Assert.Equal(testCase.Destination, message.Destination);
        Assert.Equal(testCase.Subject, message.Subject);
        Assert.Equal(testCase.Body, message.Body);
    }

    private static NotificationOutboxMessage CreateOutboxMessage(CreatingNotificationOutboxMessageTestCase testCase)
        => NotificationOutboxMessage.Create(testCase.Channel, testCase.Destination, testCase.Subject, testCase.Body);
}
