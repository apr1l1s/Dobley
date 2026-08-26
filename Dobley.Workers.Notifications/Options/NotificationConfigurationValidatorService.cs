using Dobley.Data.Core.Integrations.Telegram;
using Microsoft.Extensions.Options;

namespace Dobley.Workers.Notifications.Options;

public class NotificationConfigurationValidatorService(
    IOptions<ExpirationNotificationOptions> expirationOptions,
    ILogger<NotificationConfigurationValidatorService> logger,
    IOptions<TelegramBotOptions> telegramBotOptions,
    ITelegramBotClient telegramBotClient)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var expiration = expirationOptions.Value;
        var telegramBot = telegramBotOptions.Value;

        if (!telegramBotClient.IsConfigured)
        {
            logger.LogWarning("TELEGRAM_BOT_TOKEN не задан. Telegram bot polling и отправка уведомлений выключены.");
        }

        if (string.IsNullOrWhiteSpace(telegramBot.AllowedChatId) &&
            string.IsNullOrWhiteSpace(telegramBot.AllowedUserName))
        {
            logger.LogWarning(
                "TELEGRAM_ALLOWED_CHAT_ID и TELEGRAM_ALLOWED_USERNAME не заданы. Telegram-бот не будет отвечать пользователям.");
        }

        if (string.IsNullOrWhiteSpace(expiration.Destination))
        {
            logger.LogWarning(
                "NOTIFICATION_DESTINATION не задан. Напоминания о сроке годности не будут отправляться.");
        }

        logger.LogInformation(
            "Настройки уведомлений: пользователь Dobley {UserName}, канал {Channel}, дней до уведомления {NotifyBeforeDays}, UI {UiUrl}.",
            expiration.NotificationUserName, expiration.Channel, expiration.NotifyBeforeDays, telegramBot.UiUrl);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
