namespace Dobley.Domain.Core.Entities.Notifications;

public static class NotificationInviteCodeGenerator
{
    public static string Create()
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty)
            .ToUpperInvariant()[..12];
}
