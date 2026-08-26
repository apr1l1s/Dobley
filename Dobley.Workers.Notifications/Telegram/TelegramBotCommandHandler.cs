using Dobley.Data.Core.Integrations.Telegram;
using Dobley.Workers.Notifications.Options;
using Microsoft.Extensions.Options;

namespace Dobley.Workers.Notifications.Telegram;

public class TelegramBotCommandHandler(ITelegramBotClient telegramBotClient, ILogger<TelegramBotCommandHandler> logger,
    IOptions<TelegramBotOptions> options)
    : ITelegramBotCommandHandler
{
    private readonly TelegramBotOptions _options = options.Value;

    public async Task HandleAsync(TelegramUpdateInfo update, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(update.ChatId) || string.IsNullOrWhiteSpace(update.Text))
        {
            return;
        }

        if (update.ChatType != "private")
        {
            return;
        }

        if (!IsAllowedUser(update))
        {
            logger.LogInformation(
                "Telegram message ignored for non-allowed user. ChatId: {ChatId}, UserName: {UserName}",
                update.ChatId, update.UserName);
            return;
        }

        await telegramBotClient.SendMessageAsync(update.ChatId, CreateUiMessage(), cancellationToken);
    }

    private bool IsAllowedUser(TelegramUpdateInfo update)
    {
        if (!string.IsNullOrWhiteSpace(_options.AllowedChatId) && update.ChatId == _options.AllowedChatId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(_options.AllowedUserName) &&
            string.Equals(NormalizeUserName(update.UserName), _options.AllowedUserName,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private string CreateUiMessage()
        => "Братан, управление продуктами и уведомлениями теперь в UI.\n"
           + $"Открыть Dobley: {_options.UiUrl}";

    private static string? NormalizeUserName(string? userName)
        => userName?.Trim().TrimStart('@');
}
