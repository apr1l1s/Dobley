using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.UseCases.Notifications;

public record CreateStorageNotificationSubscriptionsCommand(int RecipientId, IReadOnlyList<int>? StorageIds,
    int NotifyBeforeDays, string UserName)
    : IUseCase<StorageNotificationSubscriptionsCommandResult>;

public record StorageNotificationSubscriptionsCommandResult(NotificationCommandStatus Status,
    IReadOnlyList<StorageNotificationSubscription> Subscriptions)
{
    public static StorageNotificationSubscriptionsCommandResult EmptyStorageIds()
        => new(NotificationCommandStatus.EmptyStorageIds, []);

    public static StorageNotificationSubscriptionsCommandResult NotFound()
        => new(NotificationCommandStatus.NotFound, []);

    public static StorageNotificationSubscriptionsCommandResult StorageNotFound()
        => new(NotificationCommandStatus.StorageNotFound, []);

    public static StorageNotificationSubscriptionsCommandResult Success(
        IReadOnlyList<StorageNotificationSubscription> subscriptions)
        => new(NotificationCommandStatus.Success, subscriptions);
}

public enum NotificationCommandStatus
{
    Success = 1,
    NotFound = 2,
    EmptyStorageIds = 3,
    StorageNotFound = 4
}

public record CreateStorageNotificationSubscriptionsCommandHandler(
    INotificationRecipientRepository NotificationRecipientRepository,
    IStorageNotificationSubscriptionRepository StorageNotificationSubscriptionRepository,
    IStorageRepository StorageRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<CreateStorageNotificationSubscriptionsCommand, StorageNotificationSubscriptionsCommandResult>
{
    public async Task<StorageNotificationSubscriptionsCommandResult> Handle(
        CreateStorageNotificationSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        if (request.StorageIds is not { Count: > 0 })
        {
            return StorageNotificationSubscriptionsCommandResult.EmptyStorageIds();
        }

        var recipient = await NotificationRecipientRepository.GetForUserAsync(request.RecipientId, request.UserName,
            cancellationToken);
        if (recipient == null)
        {
            return StorageNotificationSubscriptionsCommandResult.NotFound();
        }

        var storageIds = request.StorageIds.Distinct().ToArray();
        var ownedStorageIds = await StorageRepository.GetOwnedStorageIdsAsync(request.UserName, storageIds,
            cancellationToken);
        if (ownedStorageIds.Count != storageIds.Length)
        {
            return StorageNotificationSubscriptionsCommandResult.StorageNotFound();
        }

        var existingSubscriptions = await StorageNotificationSubscriptionRepository.GetForRecipientAsync(
            request.RecipientId, ownedStorageIds, cancellationToken);
        foreach (var subscription in existingSubscriptions.Where(x => !x.IsEnabled))
        {
            subscription.Enable();
        }

        var newSubscriptions = ownedStorageIds
            .Except(existingSubscriptions.Select(x => x.StorageId))
            .Select(storageId => StorageNotificationSubscription.Create(recipient.Id, storageId,
                request.NotifyBeforeDays))
            .ToArray();

        await StorageNotificationSubscriptionRepository.AddRangeAsync(newSubscriptions, cancellationToken);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return StorageNotificationSubscriptionsCommandResult.Success(existingSubscriptions.Concat(newSubscriptions)
            .ToArray());
    }
}
