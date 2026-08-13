using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.UseCases.Notifications;

public record DeleteNotificationRecipientCommand(int RecipientId, string UserName)
    : IUseCase<bool>;

public record DeleteNotificationRecipientCommandHandler(
    INotificationRecipientRepository NotificationRecipientRepository,
    IStorageNotificationSubscriptionRepository StorageNotificationSubscriptionRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<DeleteNotificationRecipientCommand, bool>
{
    public async Task<bool> Handle(DeleteNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await NotificationRecipientRepository
            .GetForUserAsync(request.RecipientId, request.UserName, cancellationToken);
        if (recipient == null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var subscriptions = await StorageNotificationSubscriptionRepository
            .GetForRecipientAsync(request.RecipientId, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            subscription.Delete(now);
        }

        recipient.Delete(now);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
