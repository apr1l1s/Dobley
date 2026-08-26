using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Workers.Notifications.Senders;

namespace Dobley.Workers.Notifications.Inbox;

public class NotificationInboxConsumerService(INotificationMessageConsumer notificationMessageConsumer,
    IServiceProvider services,
    NotificationSenderRegistry senderRegistry,
    ILogger<NotificationInboxConsumerService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await notificationMessageConsumer.ConsumeAsync(SendNewMessage, stoppingToken);
    }

    private async Task SendNewMessage(NotificationMessage message, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();
        var inboxRepository = scope.ServiceProvider.GetRequiredService<INotificationInboxMessageRepository>();

        if (await inboxRepository.ExistsAsync(message.MessageId, cancellationToken))
        {
            logger.LogInformation("Notification inbox message {MessageId} already processed.", message.MessageId);
            return;
        }

        await senderRegistry.GetSender(message.Channel).SendAsync(message, cancellationToken);
        await inboxRepository.AddAsync(NotificationInboxMessage.Create(message.MessageId, message.Channel,
            message.Destination), cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);
    }
}
