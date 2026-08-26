using Dobley.Data.Core.Integrations.RabbitMq;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeNotificationMessagePublisher : INotificationMessagePublisher
{
    public IReadOnlyList<NotificationMessage> PublishedMessages => _publishedMessages;

    private readonly List<NotificationMessage> _publishedMessages = [];

    public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        _publishedMessages.Add(message);
        return Task.CompletedTask;
    }
}
