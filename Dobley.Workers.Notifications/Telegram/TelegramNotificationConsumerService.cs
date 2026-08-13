using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Data.Core.Integrations.Telegram;

namespace Dobley.Workers.Notifications.Telegram;

public class TelegramNotificationConsumerService(INotificationMessageConsumer notificationMessageConsumer,
    ITelegramBotClient telegramBotClient,
    ILogger<TelegramNotificationConsumerService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!telegramBotClient.IsConfigured)
        {
            logger.LogWarning("TELEGRAM_BOT_TOKEN is empty. Telegram notifications are disabled.");
            return;
        }

        await notificationMessageConsumer.ConsumeAsync(
            async (message, cancellationToken) =>
                await telegramBotClient.SendMessageAsync(message.ExternalId, message.Text, cancellationToken),
            stoppingToken);
    }
}
