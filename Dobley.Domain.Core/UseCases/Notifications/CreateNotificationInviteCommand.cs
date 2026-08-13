using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;

namespace Dobley.Domain.Core.UseCases.Notifications;

public record CreateNotificationInviteCommand(string UserName, DateTime? ExpiresAt)
    : IUseCase<NotificationInvite>;

public record CreateNotificationInviteCommandHandler(INotificationInviteRepository NotificationInviteRepository,
    ICommonRepository CommonRepository)
    : IUseCaseHandler<CreateNotificationInviteCommand, NotificationInvite>
{
    public async Task<NotificationInvite> Handle(CreateNotificationInviteCommand request,
        CancellationToken cancellationToken)
    {
        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(1);
        var invite = NotificationInvite.Create(request.UserName, NotificationInviteCodeGenerator.Create(), expiresAt);

        await NotificationInviteRepository.AddAsync(invite, cancellationToken);
        await CommonRepository.SaveChangesAsync(cancellationToken);

        return invite;
    }
}
