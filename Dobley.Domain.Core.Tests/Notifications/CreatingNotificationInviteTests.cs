using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Notifications;

public class CreatingNotificationInviteTests
{
    [Theory]
    [ClassData(typeof(CreatingNotificationInviteTestDataGenerator))]
    public void Create_ReturnsInviteOrThrowsDomainException(CreatingNotificationInviteTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateNotificationException>(() => CreateInvite(testCase));
            return;
        }

        var invite = CreateInvite(testCase);

        Assert.Equal(testCase.ExpectedUserName, invite.UserName);
        Assert.Equal(testCase.ExpectedCode, invite.Code);
        Assert.False(invite.IsUsed);
    }

    private static NotificationInvite CreateInvite(CreatingNotificationInviteTestCase testCase)
        => NotificationInvite.Create(testCase.UserName!, testCase.Code!, testCase.ExpiresAt);
}
