namespace Dobley.Data.Core.Integrations.Telegram;

public interface ITelegramBotClient
{
    bool IsConfigured { get; }

    Task<IReadOnlyList<TelegramUpdateInfo>> GetUpdatesAsync(long offset, CancellationToken cancellationToken);

    Task SendMessageAsync(string chatId, string text, CancellationToken cancellationToken);
}
