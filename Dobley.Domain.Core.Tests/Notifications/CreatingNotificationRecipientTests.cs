using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Notifications;

public class CreatingNotificationRecipientTests
{
    [Theory]
    [ClassData(typeof(CreatingNotificationRecipientTestDataGenerator))]
    public void Create_ReturnsRecipientOrThrowsDomainException(CreatingNotificationRecipientTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateNotificationException>(() => CreateRecipient(testCase));
            return;
        }

        var recipient = CreateRecipient(testCase);

        Assert.Equal(testCase.ExpectedUserName, recipient.UserName);
        Assert.Equal(testCase.ExpectedExternalId, recipient.ExternalId);
        Assert.Equal(testCase.ExpectedChannel, recipient.Channel);
    }

    private static NotificationRecipient CreateRecipient(CreatingNotificationRecipientTestCase testCase)
        => NotificationRecipient.Create(testCase.UserName!, testCase.Channel, testCase.ExternalId!,
            testCase.DisplayName);
}
