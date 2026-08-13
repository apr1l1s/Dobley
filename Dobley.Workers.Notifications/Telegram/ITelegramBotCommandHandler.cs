using Dobley.Data.Core.Integrations.Telegram;

namespace Dobley.Workers.Notifications.Telegram;

public interface ITelegramBotCommandHandler
{
    Task HandleAsync(TelegramUpdateInfo update, CancellationToken cancellationToken);
}
