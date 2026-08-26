namespace Dobley.Data.Core.Integrations.Telegram;

public record TelegramUpdateInfo(long UpdateId, string ChatId, string? ChatType, string? UserName, string? Text,
    string? DisplayName);
