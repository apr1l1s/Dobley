using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.UseCases.Notifications;

public record DisableStorageNotificationSubscriptionsCommand(int RecipientId, string UserName)
    : IUseCase<bool>;

public record DisableStorageNotificationSubscriptionsCommandHandler(
    INotificationRecipientRepository NotificationRecipientRepository,
    IStorageNotificationSubscriptionRepository StorageNotificationSubscriptionRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<DisableStorageNotificationSubscriptionsCommand, bool>
{
    public async Task<bool> Handle(DisableStorageNotificationSubscriptionsCommand request,
        CancellationToken cancellationToken)
    {
        var recipient = await NotificationRecipientRepository.GetForUserAsync(request.RecipientId, request.UserName,
            cancellationToken);
        if (recipient == null)
        {
            return false;
        }

        var subscriptions = await StorageNotificationSubscriptionRepository.GetForRecipientAsync(request.RecipientId,
            cancellationToken);
        foreach (var subscription in subscriptions.Where(x => x.IsEnabled))
        {
            subscription.Disable();
        }

        await CommonRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
