using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Domain.Core.Tests.Builders;

public static class NotificationRecipientBuilder
{
    public static int LastId { get; private set; }

    public static NotificationRecipient Build(int? id = null, string userName = "demo",
        NotificationChannel channel = NotificationChannel.Telegram, string externalId = "123456",
        string? displayName = "Demo")
    {
        var recipient = NotificationRecipient.Create(userName, channel, externalId, displayName);
        recipient.Id = id ?? ++LastId;

        return recipient;
    }
}
