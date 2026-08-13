using Dobley.Data.Core.Integrations.RabbitMq;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeNotificationMessagePublisher : INotificationMessagePublisher
{
    public IReadOnlyList<TelegramNotificationMessage> PublishedMessages => _publishedMessages;

    private readonly List<TelegramNotificationMessage> _publishedMessages = [];

    public Task PublishAsync(TelegramNotificationMessage message, CancellationToken cancellationToken)
    {
        _publishedMessages.Add(message);
        return Task.CompletedTask;
    }
}
