using Dobley.Data.Core.Integrations.Telegram;

namespace Dobley.Workers.Notifications.Telegram;

public class TelegramBotPollingService(
    IServiceProvider services,
    ITelegramBotClient telegramBotClient,
    ILogger<TelegramBotPollingService> logger)
    : BackgroundService
{
    private long _offset;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!telegramBotClient.IsConfigured)
        {
            logger.LogWarning("TELEGRAM_BOT_TOKEN is empty. Telegram bot polling is disabled.");
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await HandleUpdates(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Telegram bot updates processing failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }
    }

    private async Task HandleUpdates(CancellationToken cancellationToken)
    {
        foreach (var update in await telegramBotClient.GetUpdatesAsync(_offset, cancellationToken))
        {
            _offset = update.UpdateId + 1;

            using var scope = services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ITelegramBotCommandHandler>();

            await handler.HandleAsync(update, cancellationToken);
        }
    }
}
