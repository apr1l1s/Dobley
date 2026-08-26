using Dobley.Data.Core.Integrations.Telegram;
using Dobley.Workers.Notifications.Options;
using Dobley.Workers.Notifications.Telegram;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Dobley.Domain.Core.Tests.Telegram;

public class TelegramBotCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithAllowedPrivateChat_SendsUiLink()
    {
        var telegramBotClient = new FakeTelegramBotClient();

        var handler = new TelegramBotCommandHandler(telegramBotClient,
            NullLogger<TelegramBotCommandHandler>.Instance, CreateOptions());

        await handler.HandleAsync(new TelegramUpdateInfo(1, "123456", "private", "apr1l1s", "/start",
            "Apr1l1s"), CancellationToken.None);

        var message = Assert.Single(telegramBotClient.SentMessages);
        Assert.Equal("123456", message.ChatId);
        Assert.Contains("https://dobley.local/ui", message.Text);
    }

    [Fact]
    public async Task HandleAsync_WithGroupChat_DoesNotSendMessage()
    {
        var telegramBotClient = new FakeTelegramBotClient();

        var handler = new TelegramBotCommandHandler(telegramBotClient,
            NullLogger<TelegramBotCommandHandler>.Instance, CreateOptions());

        await handler.HandleAsync(new TelegramUpdateInfo(1, "-100", "group", "apr1l1s", "/start",
            "Dobley chat"), CancellationToken.None);

        Assert.Empty(telegramBotClient.SentMessages);
    }

    [Fact]
    public async Task HandleAsync_WithNotAllowedPrivateChat_DoesNotSendMessage()
    {
        var telegramBotClient = new FakeTelegramBotClient();

        var handler = new TelegramBotCommandHandler(telegramBotClient,
            NullLogger<TelegramBotCommandHandler>.Instance, CreateOptions());

        await handler.HandleAsync(new TelegramUpdateInfo(1, "777", "private", "other", "/start",
            "Other"), CancellationToken.None);

        Assert.Empty(telegramBotClient.SentMessages);
    }

    private static IOptions<TelegramBotOptions> CreateOptions()
        => Options.Create(new TelegramBotOptions
        {
            AllowedChatId = "123456",
            AllowedUserName = "apr1l1s",
            UiUrl = "https://dobley.local/ui"
        });
}

file class FakeTelegramBotClient
    : ITelegramBotClient
{
    public bool IsConfigured => true;

    public List<(string ChatId, string Text)> SentMessages { get; } = [];

    public Task<IReadOnlyList<TelegramUpdateInfo>> GetUpdatesAsync(long offset,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<TelegramUpdateInfo>>([]);

    public Task SendMessageAsync(string chatId, string text, CancellationToken cancellationToken)
    {
        SentMessages.Add((chatId, text));
        return Task.CompletedTask;
    }
}
