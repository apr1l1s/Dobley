using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Workers.Notifications.ExpirationNotifications;

public class ExpirationNotificationPublishingService(
    ICommonRepository commonRepository,
    INotificationDeliveryRepository deliveryRepository,
    INotificationMessagePublisher notificationMessagePublisher,
    IProductRepository productRepository,
    IStorageNotificationSubscriptionRepository subscriptionRepository)
    : IExpirationNotificationPublishingService
{
    public async Task PublishAsync(CancellationToken cancellationToken)
    {
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
            await PublishForSubscription(subscription, products, today, cancellationToken);
        }
    }

    private async Task PublishForSubscription(StorageNotificationSubscription subscription,
        IReadOnlyList<Domain.Core.Entities.Products.Product> products, DateTime today,
        CancellationToken cancellationToken)
    {
        var recipient = subscription.DomainNotificationRecipient;
        if (recipient == null || recipient.Channel != NotificationChannel.Telegram)
        {
            return;
        }

        foreach (var product in products.Where(x => x.StorageId == subscription.StorageId))
        {
            await PublishForProduct(subscription, recipient, product, today, cancellationToken);
        }
    }

    private async Task PublishForProduct(StorageNotificationSubscription subscription,
        NotificationRecipient recipient, Domain.Core.Entities.Products.Product product, DateTime today,
        CancellationToken cancellationToken)
    {
        var expirationDate = product.ExpirationDate!.Value;
        var daysLeft = (expirationDate.Date - today).Days;
        if (daysLeft > subscription.NotifyBeforeDays)
        {
            return;
        }

        if (await deliveryRepository.ExistsAsync(recipient.Id, product.Id, expirationDate,
                recipient.Channel, cancellationToken))
        {
            return;
        }

        var storageName = product.DomainStorage?.Name ?? subscription.DomainStorage?.Name ?? "Хранилище";
        var message = ExpirationNotificationMessageFactory.Create(product.Name, storageName, expirationDate, daysLeft);

        await notificationMessagePublisher.PublishAsync(new TelegramNotificationMessage(recipient.ExternalId, message),
            cancellationToken);

        await deliveryRepository.AddAsync(NotificationDelivery.Create(recipient.Id, product.Id,
            expirationDate, recipient.Channel), cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);
    }
}
