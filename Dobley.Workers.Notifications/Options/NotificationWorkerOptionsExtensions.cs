using Dobley.Domain.Core.Entities.Notifications;
using Microsoft.Extensions.Options;

namespace Dobley.Workers.Notifications.Options;

public static class NotificationWorkerOptionsExtensions
{
    public static IServiceCollection AddNotificationWorkerOptions(this IServiceCollection services)
    {
        services
            .AddOptions<ExpirationNotificationOptions>()
            .Configure(options =>
            {
                options.Channel = GetNotificationChannel();
                options.Destination = GetTrimmedEnvironmentVariable("NOTIFICATION_DESTINATION") ??
                                      GetTrimmedEnvironmentVariable("TELEGRAM_ALLOWED_CHAT_ID");
                options.NotificationUserName = GetTrimmedEnvironmentVariable("NOTIFICATION_USER_NAME")
                                               ?? GetTrimmedEnvironmentVariable("TELEGRAM_NOTIFICATION_USER_NAME")
                                               ?? ExpirationNotificationOptions.DEFAULT_NOTIFICATION_USER_NAME;
                options.NotifyBeforeDays = GetNotifyBeforeDays();
            });

        services
            .AddOptions<TelegramBotOptions>()
            .Configure(options =>
            {
                options.AllowedChatId = GetTrimmedEnvironmentVariable("TELEGRAM_ALLOWED_CHAT_ID");
                options.AllowedUserName = NormalizeUserName(GetTrimmedEnvironmentVariable("TELEGRAM_ALLOWED_USERNAME"));
                options.UiUrl = GetTrimmedEnvironmentVariable("DOBLEY_UI_URL") ?? TelegramBotOptions.DEFAULT_UI_URL;
            });

        services.AddSingleton<IValidateOptions<ExpirationNotificationOptions>, ExpirationNotificationOptionsValidator>();
        services.AddSingleton<IValidateOptions<TelegramBotOptions>, TelegramBotOptionsValidator>();

        return services;
    }

    private static NotificationChannel GetNotificationChannel()
    {
        var value = GetTrimmedEnvironmentVariable("NOTIFICATION_CHANNEL");
        return Enum.TryParse<NotificationChannel>(value, ignoreCase: true, out var channel)
            ? channel
            : NotificationChannel.Telegram;
    }

    private static int GetNotifyBeforeDays()
    {
        var value = GetTrimmedEnvironmentVariable("DEFAULT_NOTIFY_BEFORE_DAYS");
        if (int.TryParse(value, out var days))
        {
            return Math.Clamp(days, 0, 30);
        }

        return ExpirationNotificationOptions.DEFAULT_NOTIFY_BEFORE_DAYS;
    }

    private static string? GetTrimmedEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name)?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? NormalizeUserName(string? userName)
        => userName?.Trim().TrimStart('@');
}

file class ExpirationNotificationOptionsValidator
    : IValidateOptions<ExpirationNotificationOptions>
{
    public ValidateOptionsResult Validate(string? name, ExpirationNotificationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.NotificationUserName))
        {
            return ValidateOptionsResult.Fail("NOTIFICATION_USER_NAME должен быть заполнен.");
        }

        if (options.NotifyBeforeDays is < 0 or > 30)
        {
            return ValidateOptionsResult.Fail("DEFAULT_NOTIFY_BEFORE_DAYS должен быть от 0 до 30.");
        }

        return ValidateOptionsResult.Success;
    }
}

file class TelegramBotOptionsValidator
    : IValidateOptions<TelegramBotOptions>
{
    public ValidateOptionsResult Validate(string? name, TelegramBotOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.UiUrl))
        {
            return ValidateOptionsResult.Fail("DOBLEY_UI_URL должен быть заполнен.");
        }

        return ValidateOptionsResult.Success;
    }
}
