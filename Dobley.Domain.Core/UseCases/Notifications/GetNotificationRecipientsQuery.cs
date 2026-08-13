using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.UseCases.Notifications;

public record GetNotificationRecipientsQuery(string UserName)
    : IUseCase<IReadOnlyList<NotificationRecipient>>;

public record GetNotificationRecipientsQueryHandler(INotificationRecipientRepository NotificationRecipientRepository)
    : IUseCaseHandler<GetNotificationRecipientsQuery, IReadOnlyList<NotificationRecipient>>
{
    public Task<IReadOnlyList<NotificationRecipient>> Handle(GetNotificationRecipientsQuery request,
        CancellationToken cancellationToken)
        => NotificationRecipientRepository.GetCollectionForUserAsync(request.UserName, cancellationToken);
}
