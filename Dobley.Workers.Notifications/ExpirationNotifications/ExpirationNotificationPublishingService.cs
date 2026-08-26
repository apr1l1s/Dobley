using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.Repositories.Storages;
using Dobley.Workers.Notifications.Options;
using Microsoft.Extensions.Options;

namespace Dobley.Workers.Notifications.ExpirationNotifications;

public class ExpirationNotificationPublishingService(
    ICommonRepository commonRepository,
    INotificationDeliveryRepository deliveryRepository,
    INotificationOutboxMessageRepository outboxMessageRepository,
    IProductRepository productRepository,
    IStorageRepository storageRepository,
    ILogger<ExpirationNotificationPublishingService> logger,
    IOptions<ExpirationNotificationOptions> options)
    : IExpirationNotificationPublishingService
{
    private readonly ExpirationNotificationOptions _options = options.Value;

    public async Task PublishAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Destination))
        {
            logger.LogWarning("NOTIFICATION_DESTINATION не задан. Напоминания о сроке годности выключены.");
            return;
        }

        var today = DateTime.UtcNow.Date;
        var storageIds = await storageRepository.GetStorageIdsAsync(_options.NotificationUserName, cancellationToken);

        if (storageIds.Count == 0)
        {
            logger.LogInformation(
                "Для пользователя {UserName} нет хранилищ. Напоминания о сроке годности пропущены.",
                _options.NotificationUserName);
            return;
        }

        var maxExpirationDate = today.AddDays(_options.NotifyBeforeDays + 1);
        var products = await productRepository.GetExpiringProductsAsync(storageIds, today, maxExpirationDate,
            cancellationToken);

        foreach (var product in products)
        {
            await CreateOutboxMessageForProduct(product, today, cancellationToken);
        }
    }

    private async Task CreateOutboxMessageForProduct(Domain.Core.Entities.Products.Product product, DateTime today,
        CancellationToken cancellationToken)
    {
        var expirationDate = product.ExpirationDate!.Value;
        var daysLeft = (expirationDate.Date - today).Days;
        if (daysLeft > _options.NotifyBeforeDays)
        {
            return;
        }

        if (await deliveryRepository.ExistsAsync(_options.NotificationUserName, _options.Channel,
                _options.Destination!, product.Id, expirationDate, cancellationToken))
        {
            return;
        }

        var storageName = product.DomainStorage?.Name ?? "Хранилище";
        var subject = $"Срок годности продукта {product.Name}";
        var body = ExpirationNotificationMessageFactory.Create(product.Name, storageName, expirationDate, daysLeft);

        await deliveryRepository.AddAsync(NotificationDelivery.Create(_options.NotificationUserName, _options.Channel,
            _options.Destination!, product.Id, expirationDate, subject, body), cancellationToken);

        await outboxMessageRepository.AddAsync(NotificationOutboxMessage.Create(_options.Channel,
            _options.Destination!, subject, body), cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);
    }
}
