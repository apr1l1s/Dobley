using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Workers.Notifications;

public class TelegramBotLinkingService(
    IServiceProvider services,
    IHttpClientFactory httpClientFactory,
    ILogger<TelegramBotLinkingService> logger)
    : BackgroundService
{
    private readonly TimeSpan interval = TimeSpan.FromSeconds(3);
    private long offset;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(botToken))
        {
            logger.LogWarning("TELEGRAM_BOT_TOKEN is empty. Telegram bot linking is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await HandleUpdates(botToken, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Telegram bot updates processing failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task HandleUpdates(string botToken, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.GetFromJsonAsync<TelegramUpdatesResponse>(
            $"https://api.telegram.org/bot{botToken}/getUpdates?timeout=1&offset={offset}", cancellationToken);

        if (response?.Result == null)
        {
            return;
        }

        foreach (var update in response.Result)
        {
            offset = update.UpdateId + 1;

            var chatId = update.Message?.Chat?.Id.ToString();
            var text = update.Message?.Text;
            if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (TryGetStartCode(text, out var code))
            {
                await LinkRecipient(botToken, chatId, update.Message!.Chat.CreateDisplayName(), code,
                    cancellationToken);
                continue;
            }

            if (IsCommand(text, "/invite"))
            {
                await CreateInvite(botToken, chatId, cancellationToken);
                continue;
            }

            if (IsCommand(text, "/sub"))
            {
                await Subscribe(botToken, chatId, cancellationToken);
                continue;
            }

            if (IsCommand(text, "/unsub"))
            {
                await Unsubscribe(botToken, chatId, cancellationToken);
                continue;
            }

            await SendMessage(botToken, chatId, CreateHelpMessage(), cancellationToken);
        }
    }

    private async Task CreateInvite(string botToken, string chatId, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();
        var inviteRepository = scope.ServiceProvider.GetRequiredService<INotificationInviteRepository>();
        var recipientRepository = scope.ServiceProvider.GetRequiredService<INotificationRecipientRepository>();

        var recipient = await recipientRepository.GetByChannelAndExternalIdAsync(NotificationChannel.Telegram, chatId,
            cancellationToken);

        if (recipient == null)
        {
            await SendMessage(botToken, chatId, "Сначала подключи Telegram командой /start <код>.", cancellationToken);
            return;
        }

        var invite = NotificationInvite.Create(recipient.UserName, NotificationInviteCodeGenerator.Create(),
            DateTime.UtcNow.AddDays(1));

        await inviteRepository.AddAsync(invite, cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await SendMessage(botToken, chatId,
            $"Код приглашения: {invite.Code}\nДействует до {invite.ExpiresAt:dd.MM.yyyy HH:mm} UTC.\nОтправь его человеку, он подключится командой /start {invite.Code}.",
            cancellationToken);
    }

    private async Task LinkRecipient(string botToken, string chatId, string? displayName, string code,
        CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();
        var inviteRepository = scope.ServiceProvider.GetRequiredService<INotificationInviteRepository>();
        var recipientRepository = scope.ServiceProvider.GetRequiredService<INotificationRecipientRepository>();
        var now = DateTime.UtcNow;

        var invite = await inviteRepository.GetByCodeAsync(code, cancellationToken);

        if (invite == null || !invite.CanBeUsed(now))
        {
            await SendMessage(botToken, chatId, "Код подключения не найден или уже истек.", cancellationToken);
            return;
        }

        var recipient = await recipientRepository.GetByChannelAndExternalIdAsync(NotificationChannel.Telegram, chatId,
            cancellationToken);

        if (recipient == null)
        {
            recipient = NotificationRecipient.Create(invite.UserName, NotificationChannel.Telegram, chatId,
                displayName);
            await recipientRepository.AddAsync(recipient, cancellationToken);
            await commonRepository.SaveChangesAsync(cancellationToken);
        }
        else if (recipient.UserName != invite.UserName)
        {
            await SendMessage(botToken, chatId, "Этот Telegram-чат уже подключен к другому профилю.",
                cancellationToken);
            return;
        }
        else
        {
            recipient.UpdateDisplayName(displayName);
        }

        await SubscribeRecipient(scope.ServiceProvider, recipient.Id, recipient.UserName, cancellationToken);
        invite.MarkUsed(now);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await SendMessage(botToken, chatId, "Готово, рассылка уведомлений подключена.", cancellationToken);
    }

    private async Task Subscribe(string botToken, string chatId, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();
        var recipientRepository = scope.ServiceProvider.GetRequiredService<INotificationRecipientRepository>();
        var recipient = await recipientRepository.GetByChannelAndExternalIdAsync(NotificationChannel.Telegram, chatId,
            cancellationToken);

        if (recipient == null)
        {
            await SendMessage(botToken, chatId, "Сначала подключи Telegram командой /start <код>.", cancellationToken);
            return;
        }

        var affectedCount = await SubscribeRecipient(scope.ServiceProvider, recipient.Id, recipient.UserName,
            cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await SendMessage(botToken, chatId,
            affectedCount == 0
                ? "В профиле пока нет хранилищ для рассылки."
                : $"Готово, рассылка включена. Хранилищ в рассылке: {affectedCount}.",
            cancellationToken);
    }

    private async Task Unsubscribe(string botToken, string chatId, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();
        var recipientRepository = scope.ServiceProvider.GetRequiredService<INotificationRecipientRepository>();
        var subscriptionRepository =
            scope.ServiceProvider.GetRequiredService<IStorageNotificationSubscriptionRepository>();

        var recipient = await recipientRepository.GetByChannelAndExternalIdAsync(NotificationChannel.Telegram, chatId,
            cancellationToken);
        if (recipient == null)
        {
            await SendMessage(botToken, chatId, "Сначала подключи Telegram командой /start <код>.", cancellationToken);
            return;
        }

        var subscriptions = await subscriptionRepository.GetForRecipientAsync(recipient.Id, cancellationToken);
        var enabledSubscriptions = subscriptions.Where(x => x.IsEnabled).ToArray();
        foreach (var subscription in enabledSubscriptions)
        {
            subscription.Disable();
        }

        await commonRepository.SaveChangesAsync(cancellationToken);

        await SendMessage(botToken, chatId,
            enabledSubscriptions.Length == 0
                ? "Активной рассылки для этого Telegram-чата нет."
                : "Готово, рассылка уведомлений выключена.",
            cancellationToken);
    }

    private static async Task<int> SubscribeRecipient(IServiceProvider serviceProvider, int recipientId,
        string userName, CancellationToken cancellationToken)
    {
        var storageRepository = serviceProvider.GetRequiredService<IStorageRepository>();
        var subscriptionRepository =
            serviceProvider.GetRequiredService<IStorageNotificationSubscriptionRepository>();
        var defaultNotifyBeforeDays = GetDefaultNotifyBeforeDays();
        var storageIds = await storageRepository.GetStorageIdsAsync(userName, cancellationToken);

        var existingSubscriptions = await subscriptionRepository.GetForRecipientAsync(recipientId, storageIds,
            cancellationToken);
        foreach (var subscription in existingSubscriptions.Where(x => !x.IsEnabled))
        {
            subscription.Enable();
        }

        var subscriptions = storageIds
            .Except(existingSubscriptions.Select(x => x.StorageId))
            .Select(storageId => StorageNotificationSubscription.Create(recipientId, storageId,
                defaultNotifyBeforeDays))
            .ToArray();

        await subscriptionRepository.AddRangeAsync(subscriptions, cancellationToken);

        return storageIds.Count;
    }

    private async Task SendMessage(string botToken, string chatId, string text, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{botToken}/sendMessage",
            new
            {
                chat_id = chatId,
                text
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static string CreateHelpMessage()
        => "Команды бота:\n"
           + "/start <код> - подключить Telegram к профилю.\n"
           + "/invite - создать новый код приглашения для этого профиля.\n"
           + "/sub - включить рассылку уведомлений.\n"
           + "/unsub - выключить рассылку уведомлений.\n"
           + "/help - показать команды.";

    private static int GetDefaultNotifyBeforeDays()
        => int.TryParse(Environment.GetEnvironmentVariable("DEFAULT_NOTIFY_BEFORE_DAYS"), out var days)
            ? days
            : 3;

    private static bool IsCommand(string text, string command)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 1 && IsCommandToken(parts[0], command);
    }

    private static bool TryGetStartCode(string text, out string code)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && IsCommandToken(parts[0], "/start"))
        {
            code = parts[1].ToUpperInvariant();
            return true;
        }

        code = string.Empty;
        return false;
    }

    private static bool IsCommandToken(string token, string command)
        => token.Equals(command, StringComparison.OrdinalIgnoreCase)
           || token.StartsWith($"{command}@", StringComparison.OrdinalIgnoreCase);
}

public record TelegramUpdatesResponse([property: JsonPropertyName("result")] IReadOnlyList<TelegramUpdate> Result);

public record TelegramUpdate(
    [property: JsonPropertyName("update_id")] long UpdateId,
    [property: JsonPropertyName("message")] TelegramMessage? Message);

public record TelegramMessage(
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("chat")] TelegramChat Chat);

public record TelegramChat(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("username")] string? UserName)
{
    public string? CreateDisplayName()
        => string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
