using Dobley.Domain.Core.Tests.Builders;
using Dobley.Domain.Core.Tests.UseCases.Fakes;
using Dobley.Domain.Core.UseCases.Notifications;

namespace Dobley.Domain.Core.Tests.UseCases.Notifications;

public class NotificationUseCaseTests
{
    [Fact]
    public async Task CreateNotificationInviteCommand_CreatesInviteAndSavesChanges()
    {
        var inviteRepository = new FakeNotificationInviteRepository();
        var commonRepository = new FakeCommonRepository();
        var handler = new CreateNotificationInviteCommandHandler(inviteRepository, commonRepository);

        var invite = await handler.Handle(new CreateNotificationInviteCommand("demo", null), CancellationToken.None);

        Assert.Equal("demo", invite.UserName);
        Assert.False(string.IsNullOrWhiteSpace(invite.Code));
        Assert.Single(inviteRepository.AddedInvites);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateStorageNotificationSubscriptionsCommand_WithOwnedStorages_CreatesSubscriptions()
    {
        var recipient = NotificationRecipientBuilder.Build(id: 1, userName: "demo");
        var firstStorage = StorageBuilder.Build(id: 1, userName: "demo");
        var secondStorage = StorageBuilder.Build(id: 2, userName: "demo");
        var recipientRepository = new FakeNotificationRecipientRepository(recipient);
        var subscriptionRepository = new FakeStorageNotificationSubscriptionRepository();
        var storageRepository = new FakeStorageRepository(firstStorage, secondStorage);
        var commonRepository = new FakeCommonRepository();
        var handler = new CreateStorageNotificationSubscriptionsCommandHandler(recipientRepository,
            subscriptionRepository, storageRepository, commonRepository);

        var result = await handler.Handle(new CreateStorageNotificationSubscriptionsCommand(recipient.Id,
            [firstStorage.Id, secondStorage.Id], 3, "demo"), CancellationToken.None);

        Assert.Equal(NotificationCommandStatus.Success, result.Status);
        Assert.Equal(2, result.Subscriptions.Count);
        Assert.Equal(2, subscriptionRepository.AddedSubscriptions.Count);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateStorageNotificationSubscriptionsCommand_WithEmptyStorages_ReturnsEmptyStorageStatus()
    {
        var handler = new CreateStorageNotificationSubscriptionsCommandHandler(
            new FakeNotificationRecipientRepository(), new FakeStorageNotificationSubscriptionRepository(),
            new FakeStorageRepository(), new FakeCommonRepository());

        var result = await handler.Handle(new CreateStorageNotificationSubscriptionsCommand(1, [], 3, "demo"),
            CancellationToken.None);

        Assert.Equal(NotificationCommandStatus.EmptyStorageIds, result.Status);
    }

    [Fact]
    public async Task DisableStorageNotificationSubscriptionsCommand_DisablesEnabledSubscriptions()
    {
        var recipient = NotificationRecipientBuilder.Build(id: 1, userName: "demo");
        var subscription = StorageNotificationSubscriptionBuilder.Build(notificationRecipientId: recipient.Id);
        var commonRepository = new FakeCommonRepository();
        var handler = new DisableStorageNotificationSubscriptionsCommandHandler(
            new FakeNotificationRecipientRepository(recipient),
            new FakeStorageNotificationSubscriptionRepository(subscription),
            commonRepository);

        var result = await handler.Handle(new DisableStorageNotificationSubscriptionsCommand(recipient.Id, "demo"),
            CancellationToken.None);

        Assert.True(result);
        Assert.False(subscription.IsEnabled);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteNotificationRecipientCommand_SoftDeletesRecipientAndSubscriptions()
    {
        var recipient = NotificationRecipientBuilder.Build(id: 1, userName: "demo");
        var subscription = StorageNotificationSubscriptionBuilder.Build(notificationRecipientId: recipient.Id);
        var commonRepository = new FakeCommonRepository();
        var handler = new DeleteNotificationRecipientCommandHandler(
            new FakeNotificationRecipientRepository(recipient),
            new FakeStorageNotificationSubscriptionRepository(subscription),
            commonRepository);

        var result = await handler.Handle(new DeleteNotificationRecipientCommand(recipient.Id, "demo"),
            CancellationToken.None);

        Assert.True(result);
        Assert.True(recipient.IsDeleted);
        Assert.True(subscription.IsDeleted);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }
}
