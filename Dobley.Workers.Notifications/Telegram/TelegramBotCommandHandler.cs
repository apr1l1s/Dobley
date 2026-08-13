using Dobley.Data.Core.Integrations.Telegram;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Workers.Notifications.Telegram;

public class TelegramBotCommandHandler(
    ICommonRepository commonRepository,
    INotificationInviteRepository inviteRepository,
    INotificationRecipientRepository recipientRepository,
    IStorageNotificationSubscriptionRepository subscriptionRepository,
    IStorageRepository storageRepository,
    ITelegramBotClient telegramBotClient)
    : ITelegramBotCommandHandler
{
    public async Task HandleAsync(TelegramUpdateInfo update, CancellationToken cancellationToken)
    {
        var chatId = update.ChatId;
        var text = update.Text;
        if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (TryGetStartCode(text, out var code))
        {
            await LinkRecipient(chatId, update.DisplayName, code, cancellationToken);
            return;
        }

        if (IsCommand(text, "/invite"))
        {
            await CreateInvite(chatId, cancellationToken);
            return;
        }

        if (IsCommand(text, "/sub"))
        {
            await Subscribe(chatId, cancellationToken);
            return;
        }

        if (IsCommand(text, "/unsub"))
        {
            await Unsubscribe(chatId, cancellationToken);
            return;
        }

        if (TryGetCommandArgument(text, "/unlink", out var unlinkCode))
        {
            await UnlinkByCode(chatId, unlinkCode, cancellationToken);
            return;
        }

        if (IsCommand(text, "/unlink"))
        {
            await Unlink(chatId, cancellationToken);
            return;
        }

        if (IsCommand(text, "/help"))
        {
            await telegramBotClient.SendMessageAsync(chatId, CreateHelpMessage(), cancellationToken);
            return;
        }

        if (!IsGroupChat(update.ChatType))
        {
            await telegramBotClient.SendMessageAsync(chatId, CreateHelpMessage(), cancellationToken);
        }
    }

    private async Task CreateInvite(string chatId, CancellationToken cancellationToken)
    {
        var recipient = await GetSingleRecipient(chatId, cancellationToken);

        if (recipient == null)
        {
            return;
        }

        var invite = NotificationInvite.Create(recipient.UserName, NotificationInviteCodeGenerator.Create(),
            DateTime.UtcNow.AddDays(1));

        await inviteRepository.AddAsync(invite, cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await telegramBotClient.SendMessageAsync(chatId,
            $"Код приглашения: {invite.Code}\nДействует до {invite.ExpiresAt:dd.MM.yyyy HH:mm} UTC.\nОтправь его человеку, он подключится командой /start {invite.Code}.",
            cancellationToken);
    }

    private async Task LinkRecipient(string chatId, string? displayName, string code,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var invite = await inviteRepository.GetByCodeAsync(code, cancellationToken);

        if (invite == null || !invite.CanBeUsed(now))
        {
            await telegramBotClient.SendMessageAsync(chatId, "Код подключения не найден или уже истек.", cancellationToken);
            return;
        }

        var recipient = await recipientRepository.GetForUserAsync(invite.UserName, NotificationChannel.Telegram,
            chatId, cancellationToken);

        if (recipient == null)
        {
            recipient = NotificationRecipient.Create(invite.UserName, NotificationChannel.Telegram, chatId,
                displayName);
            await recipientRepository.AddAsync(recipient, cancellationToken);
            await commonRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            recipient.UpdateDisplayName(displayName);
        }

        await SubscribeRecipient(recipient.Id, recipient.UserName, cancellationToken);
        invite.MarkUsed(now);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await telegramBotClient.SendMessageAsync(chatId, "Готово, рассылка уведомлений подключена.", cancellationToken);
    }

    private async Task Subscribe(string chatId, CancellationToken cancellationToken)
    {
        var recipient = await GetSingleRecipient(chatId, cancellationToken);

        if (recipient == null)
        {
            return;
        }

        var affectedCount = await SubscribeRecipient(recipient.Id, recipient.UserName, cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await telegramBotClient.SendMessageAsync(chatId,
            affectedCount == 0
                ? "В профиле пока нет хранилищ для рассылки."
                : $"Готово, рассылка включена. Хранилищ в рассылке: {affectedCount}.",
            cancellationToken);
    }

    private async Task Unsubscribe(string chatId, CancellationToken cancellationToken)
    {
        var recipient = await GetSingleRecipient(chatId, cancellationToken);
        if (recipient == null)
        {
            return;
        }

        var subscriptions = await subscriptionRepository.GetForRecipientAsync(recipient.Id, cancellationToken);
        var enabledSubscriptions = subscriptions.Where(x => x.IsEnabled).ToArray();
        foreach (var subscription in enabledSubscriptions)
        {
            subscription.Disable();
        }

        await commonRepository.SaveChangesAsync(cancellationToken);

        await telegramBotClient.SendMessageAsync(chatId,
            enabledSubscriptions.Length == 0
                ? "Активной рассылки для этого Telegram-чата нет."
                : "Готово, рассылка уведомлений выключена.",
            cancellationToken);
    }

    private async Task Unlink(string chatId, CancellationToken cancellationToken)
    {
        var recipient = await GetSingleRecipient(chatId, cancellationToken);
        if (recipient == null)
        {
            return;
        }

        await UnlinkRecipient(recipient, cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await telegramBotClient.SendMessageAsync(chatId, "Готово, Telegram-чат отвязан от профиля.", cancellationToken);
    }

    private async Task UnlinkByCode(string chatId, string code, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var invite = await inviteRepository.GetByCodeAsync(code, cancellationToken);
        if (invite == null || !invite.CanBeUsed(now))
        {
            await telegramBotClient.SendMessageAsync(chatId, "Код отключения не найден или уже истек.",
                cancellationToken);
            return;
        }

        var recipient = await recipientRepository.GetForUserAsync(invite.UserName, NotificationChannel.Telegram,
            chatId, cancellationToken);
        if (recipient == null)
        {
            await telegramBotClient.SendMessageAsync(chatId, "Этот Telegram-чат не подключен к профилю из кода.",
                cancellationToken);
            return;
        }

        await UnlinkRecipient(recipient, cancellationToken);
        invite.MarkUsed(now);
        await commonRepository.SaveChangesAsync(cancellationToken);

        await telegramBotClient.SendMessageAsync(chatId, "Готово, Telegram-чат отвязан от профиля из кода.",
            cancellationToken);
    }

    private async Task<int> SubscribeRecipient(int recipientId, string userName, CancellationToken cancellationToken)
    {
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

    private async Task UnlinkRecipient(NotificationRecipient recipient, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var subscriptions = await subscriptionRepository.GetForRecipientAsync(recipient.Id, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            subscription.Delete(now);
        }

        recipient.Delete(now);
    }

    private async Task<NotificationRecipient?> GetSingleRecipient(string chatId, CancellationToken cancellationToken)
    {
        var recipients = await recipientRepository.GetCollectionByChannelAndExternalIdAsync(
            NotificationChannel.Telegram, chatId, cancellationToken);

        if (recipients.Count == 0)
        {
            await telegramBotClient.SendMessageAsync(chatId, "Сначала подключи Telegram командой /start <код>.",
                cancellationToken);
            return null;
        }

        if (recipients.Count > 1)
        {
            await telegramBotClient.SendMessageAsync(chatId,
                "Этот Telegram-чат подключен к нескольким профилям. Чтобы выбрать профиль, создай код в UI нужного аккаунта и отправь /start <код>.",
                cancellationToken);
            return null;
        }

        return recipients[0];
    }

    private static string CreateHelpMessage()
        => "Команды бота:\n"
           + "/start <код> - подключить Telegram к профилю.\n"
           + "/invite - создать новый код приглашения для этого профиля.\n"
           + "/sub - включить рассылку уведомлений.\n"
           + "/unsub - выключить рассылку уведомлений.\n"
           + "/unlink - отвязать Telegram-чат от профиля.\n"
           + "/unlink <код> - отвязать чат от конкретного профиля.\n"
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
        => TryGetCommandArgument(text, "/start", out code);

    private static bool TryGetCommandArgument(string text, string command, out string argument)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && IsCommandToken(parts[0], command))
        {
            argument = parts[1].ToUpperInvariant();
            return true;
        }

        argument = string.Empty;
        return false;
    }

    private static bool IsCommandToken(string token, string command)
        => token.Equals(command, StringComparison.OrdinalIgnoreCase)
           || token.StartsWith($"{command}@", StringComparison.OrdinalIgnoreCase);

    private static bool IsGroupChat(string? chatType)
        => chatType is "group" or "supergroup" or "channel";
}
