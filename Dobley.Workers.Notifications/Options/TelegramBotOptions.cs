namespace Dobley.Workers.Notifications.Options;

public class TelegramBotOptions
{
    public const string DEFAULT_UI_URL = "https://dobley.local/ui";

    public string? AllowedChatId { get; set; }

    public string? AllowedUserName { get; set; }

    public string UiUrl { get; set; } = DEFAULT_UI_URL;
}
