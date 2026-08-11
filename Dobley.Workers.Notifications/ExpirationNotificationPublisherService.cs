using System.Text;
using System.Text.Json;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Products;
using RabbitMQ.Client;

namespace Dobley.Workers.Notifications;

public class ExpirationNotificationPublisherService(
    IServiceProvider services,
    RabbitMqOptions rabbitMqOptions,
    ILogger<ExpirationNotificationPublisherService> logger)
    : BackgroundService
{
    private readonly HashSet<string> publishedKeys = [];
    private readonly TimeSpan interval = GetInterval();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Expiration notification watcher started with interval {IntervalSeconds} seconds.",
            interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishExpirationNotifications(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Expiration notification publishing failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task PublishExpirationNotifications(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var subscriptionRepository =
            scope.ServiceProvider.GetRequiredService<IStorageNotificationSubscriptionRepository>();
        var today = DateTime.UtcNow.Date;

        var subscriptions = await subscriptionRepository.GetEnabledSubscriptionsAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            return;
        }

        var storageIds = subscriptions.Select(x => x.StorageId).Distinct().ToArray();
        var maxNotifyBeforeDays = subscriptions.Max(x => x.NotifyBeforeDays);
        var maxExpirationDate = today.AddDays(maxNotifyBeforeDays + 1);

        var products = await productRepository.GetExpiringProductsAsync(storageIds, today, maxExpirationDate,
            cancellationToken);

        foreach (var subscription in subscriptions)
        {
            var recipient = subscription.DomainNotificationRecipient;
            if (recipient == null || recipient.Channel.ToString() != "Telegram")
            {
                continue;
            }

            foreach (var product in products.Where(x => x.StorageId == subscription.StorageId))
            {
                var daysLeft = (product.ExpirationDate!.Value.Date - today).Days;
                if (daysLeft > subscription.NotifyBeforeDays)
                {
                    continue;
                }

                var publishKey = $"{recipient.Id}:{product.Id}:{product.ExpirationDate.Value:yyyyMMdd}";
                if (!publishedKeys.Add(publishKey))
                {
                    continue;
                }

                Publish(new TelegramNotificationMessage(recipient.ExternalId, CreateMessage(product.Name,
                    product.DomainStorage?.Name ?? subscription.DomainStorage?.Name ?? "Хранилище",
                    product.ExpirationDate.Value, daysLeft)));
            }
        }
    }

    private void Publish(TelegramNotificationMessage message)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(rabbitMqOptions.QueueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: string.Empty, routingKey: rabbitMqOptions.QueueName, basicProperties: properties,
            body: body);

        logger.LogInformation("Published expiration notification for Telegram chat {ExternalId}", message.ExternalId);
    }

    private static string CreateMessage(string productName, string storageName, DateTime expirationDate, int daysLeft)
        => $"Братан, у продукта \"{productName}\" скоро закончится срок годности.\n"
           + $"Хранилище: {storageName}.\n"
           + $"Дата: {expirationDate:dd.MM.yyyy}.\n"
           + $"Осталось дней: {daysLeft}.";

    private static TimeSpan GetInterval()
        => int.TryParse(Environment.GetEnvironmentVariable("EXPIRATION_WATCH_INTERVAL_SECONDS"), out var seconds)
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromHours(6);
}
