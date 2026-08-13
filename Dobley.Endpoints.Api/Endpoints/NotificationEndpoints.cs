using System.Security.Claims;
using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Notifications;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Dobley.Endpoints.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notificationsApi = app.MapGroup("/notifications").RequireAuthorization();

        notificationsApi.MapPost("/invites/create", async ([FromBody] CreateNotificationInviteRequest? request,
            ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var invite = await dispatcher.DispatchAsync(
                new CreateNotificationInviteCommand(user.GetCurrentUserName(), request?.ExpiresAt),
                cancellationToken);

            return Results.Created($"/notifications/invites/{invite.Id}", NotificationInviteResponse.Create(invite));
        });

        notificationsApi.MapGet("/recipients", async (ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => Results.Ok((await dispatcher.DispatchAsync(new GetNotificationRecipientsQuery(user.GetCurrentUserName()),
                    cancellationToken))
                .Select(NotificationRecipientResponse.Create)));

        notificationsApi.MapPost("/recipients/{recipientId}/subscriptions", async (int recipientId,
            [FromBody] StorageNotificationSubscriptionRequest? request, ClaimsPrincipal user,
            [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(new CreateStorageNotificationSubscriptionsCommand(recipientId,
                request?.StorageIds, request?.NotifyBeforeDays ?? 3, user.GetCurrentUserName()), cancellationToken);

            return result.Status switch
            {
                NotificationCommandStatus.Success => Results.Ok(result.Subscriptions
                    .Select(StorageNotificationSubscriptionResponse.Create)),
                NotificationCommandStatus.EmptyStorageIds => Results.BadRequest(new
                {
                    error = "Необходимо указать хотя бы одно хранилище"
                }),
                NotificationCommandStatus.StorageNotFound => Results.BadRequest(new
                {
                    error = "Одно или несколько хранилищ не найдены"
                }),
                _ => Results.NotFound()
            };
        });

        notificationsApi.MapDelete("/recipients/{recipientId}/subscriptions", async (int recipientId,
                ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(
                new DisableStorageNotificationSubscriptionsCommand(recipientId, user.GetCurrentUserName()),
                cancellationToken)
                ? Results.NoContent()
                : Results.NotFound());

        notificationsApi.MapDelete("/recipients/{recipientId}", async (int recipientId, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new DeleteNotificationRecipientCommand(recipientId,
                    user.GetCurrentUserName()), cancellationToken)
                ? Results.NoContent()
                : Results.NotFound());

        return app;
    }
}
