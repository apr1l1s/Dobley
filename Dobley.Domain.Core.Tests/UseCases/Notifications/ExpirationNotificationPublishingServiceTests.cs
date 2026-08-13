using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Tests.Builders;
using Dobley.Domain.Core.Tests.UseCases.Fakes;
using Dobley.Workers.Notifications.ExpirationNotifications;

namespace Dobley.Domain.Core.Tests.UseCases.Notifications;

public class ExpirationNotificationPublishingServiceTests
{
    [Fact]
    public async Task PublishAsync_WithExpiringProduct_PublishesMessageAndSavesDelivery()
    {
        var storage = StorageBuilder.Build(id: 1, name: "Home fridge", userName: "demo");
        var product = ProductBuilder.Build(id: 1, name: "Bread", expirationDate: DateTime.UtcNow.AddDays(2),
            storageId: storage.Id);
        product.DomainStorage = storage;
        var recipient = NotificationRecipientBuilder.Build(id: 1, externalId: "123456");
        var subscription = StorageNotificationSubscriptionBuilder.Build(notificationRecipientId: recipient.Id,
            storageId: storage.Id, notifyBeforeDays: 3);
        subscription.SetRecipient(recipient);
        subscription.SetStorage(storage);
        var publisher = new FakeNotificationMessagePublisher();
        var deliveryRepository = new FakeNotificationDeliveryRepository();
        var commonRepository = new FakeCommonRepository();
        var service = new ExpirationNotificationPublishingService(commonRepository, deliveryRepository, publisher,
            new FakeProductRepository(product), new FakeStorageNotificationSubscriptionRepository(subscription));

        await service.PublishAsync(CancellationToken.None);

        var message = Assert.Single(publisher.PublishedMessages);
        Assert.Equal(recipient.ExternalId, message.ExternalId);
        Assert.Contains("Bread", message.Text);
        Assert.Contains("Home fridge", message.Text);
        Assert.Single(deliveryRepository.AddedDeliveries);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task PublishAsync_WithExistingDelivery_DoesNotPublishDuplicateMessage()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "demo");
        var expirationDate = DateTime.UtcNow.AddDays(2);
        var product = ProductBuilder.Build(id: 1, expirationDate: expirationDate, storageId: storage.Id);
        product.DomainStorage = storage;
        var recipient = NotificationRecipientBuilder.Build(id: 1);
        var subscription = StorageNotificationSubscriptionBuilder.Build(notificationRecipientId: recipient.Id,
            storageId: storage.Id, notifyBeforeDays: 3);
        subscription.SetRecipient(recipient);
        subscription.SetStorage(storage);
        var delivery = NotificationDelivery.Create(recipient.Id, product.Id, expirationDate,
            NotificationChannel.Telegram);
        var publisher = new FakeNotificationMessagePublisher();
        var commonRepository = new FakeCommonRepository();
        var service = new ExpirationNotificationPublishingService(commonRepository,
            new FakeNotificationDeliveryRepository(delivery), publisher, new FakeProductRepository(product),
            new FakeStorageNotificationSubscriptionRepository(subscription));

        await service.PublishAsync(CancellationToken.None);

        Assert.Empty(publisher.PublishedMessages);
        Assert.Equal(0, commonRepository.SaveChangesCount);
    }
}
