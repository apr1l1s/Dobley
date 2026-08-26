using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Tests.Builders;
using Dobley.Domain.Core.Tests.UseCases.Fakes;
using Dobley.Workers.Notifications.ExpirationNotifications;
using Dobley.Workers.Notifications.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Dobley.Domain.Core.Tests.UseCases.Notifications;

public class ExpirationNotificationPublishingServiceTests
{
    [Fact]
    public async Task PublishAsync_WithExpiringProduct_CreatesOutboxMessageAndSavesDelivery()
    {
        var storage = StorageBuilder.Build(id: 1, name: "Home fridge", userName: "demo");
        var product = ProductBuilder.Build(id: 1, name: "Bread", expirationDate: DateTime.UtcNow.AddDays(2),
            storageId: storage.Id);
        product.DomainStorage = storage;
        var outboxRepository = new FakeNotificationOutboxMessageRepository();
        var deliveryRepository = new FakeNotificationDeliveryRepository();
        var commonRepository = new FakeCommonRepository();

        var service = new ExpirationNotificationPublishingService(commonRepository, deliveryRepository,
            outboxRepository, new FakeProductRepository(product), new FakeStorageRepository(storage),
            NullLogger<ExpirationNotificationPublishingService>.Instance, CreateOptions());

        await service.PublishAsync(CancellationToken.None);

        var message = Assert.Single(outboxRepository.AddedMessages);
        Assert.Equal(NotificationChannel.Telegram, message.Channel);
        Assert.Equal("123456", message.Destination);
        Assert.Contains("Bread", message.Body);
        Assert.Contains("Home fridge", message.Body);
        Assert.Single(deliveryRepository.AddedDeliveries);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task PublishAsync_WithExistingDelivery_DoesNotCreateDuplicateOutboxMessage()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "demo");
        var expirationDate = DateTime.UtcNow.AddDays(2);
        var product = ProductBuilder.Build(id: 1, expirationDate: expirationDate, storageId: storage.Id);
        product.DomainStorage = storage;
        var delivery = NotificationDelivery.Create("demo", NotificationChannel.Telegram, "123456",
            product.Id, expirationDate, "Срок годности продукта", "Текст уведомления");
        var outboxRepository = new FakeNotificationOutboxMessageRepository();
        var commonRepository = new FakeCommonRepository();

        var service = new ExpirationNotificationPublishingService(commonRepository,
            new FakeNotificationDeliveryRepository(delivery), outboxRepository, new FakeProductRepository(product),
            new FakeStorageRepository(storage), NullLogger<ExpirationNotificationPublishingService>.Instance,
            CreateOptions());

        await service.PublishAsync(CancellationToken.None);

        Assert.Empty(outboxRepository.AddedMessages);
        Assert.Equal(0, commonRepository.SaveChangesCount);
    }

    private static IOptions<ExpirationNotificationOptions> CreateOptions()
        => Options.Create(new ExpirationNotificationOptions
        {
            Channel = NotificationChannel.Telegram,
            Destination = "123456",
            NotificationUserName = "demo",
            NotifyBeforeDays = 3
        });
}
