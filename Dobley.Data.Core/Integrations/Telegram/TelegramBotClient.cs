using System.Net.Http.Json;
using Dobley.Data.Core.Integrations.Telegram.Dto;

namespace Dobley.Data.Core.Integrations.Telegram;

public class TelegramBotClient(IHttpClientFactory httpClientFactory)
    : ITelegramBotClient
{
    private readonly string? _botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_botToken);

    public async Task<IReadOnlyList<TelegramUpdateInfo>> GetUpdatesAsync(long offset,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return [];
        }

        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.GetFromJsonAsync<TelegramUpdatesResponse>(
            $"https://api.telegram.org/bot{_botToken}/getUpdates?timeout=1&offset={offset}", cancellationToken);

        return response?.Result
            .Where(x => x.Message?.Chat != null)
            .Select(x => new TelegramUpdateInfo(
                x.UpdateId,
                x.Message!.Chat.Id.ToString(),
                x.Message.Chat.Type,
                x.Message.Chat.UserName,
                x.Message.Text,
                x.Message.Chat.CreateDisplayName()))
            .ToArray() ?? [];
    }

    public async Task SendMessageAsync(string chatId, string text, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return;
        }

        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient
            .PostAsJsonAsync($"https://api.telegram.org/bot{_botToken}/sendMessage", new { chat_id = chatId, text },
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
