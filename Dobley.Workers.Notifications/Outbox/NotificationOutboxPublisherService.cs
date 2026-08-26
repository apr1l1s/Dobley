using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Workers.Notifications.Outbox;

public class NotificationOutboxPublisherService(IServiceProvider services,
    ILogger<NotificationOutboxPublisherService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification outbox publisher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessages(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification outbox publishing failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task PublishPendingMessages(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<INotificationOutboxMessageRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<INotificationMessagePublisher>();

        foreach (var message in await outboxRepository.GetPendingAsync(20, cancellationToken))
        {
            try
            {
                await publisher.PublishAsync(new NotificationMessage(message.MessageId, message.Channel,
                    message.Destination, message.Subject, message.Body), cancellationToken);
                message.MarkPublished(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification outbox message {MessageId} publishing failed",
                    message.MessageId);
                message.MarkFailed(exception.Message);
            }
        }

        await commonRepository.SaveChangesAsync(cancellationToken);
    }
}
