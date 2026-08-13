using System.Security.Claims;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Storages;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Dobley.Endpoints.Api.Endpoints;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var storagesApi = app.MapGroup("/storages").RequireAuthorization();

        storagesApi.MapGet("/", async ([FromQuery] int? pageIndex, [FromQuery] int? pageSize, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => Results.Ok(PaginatedResponse<StorageResponse>.Create(
                await dispatcher.DispatchAsync(new GetStoragesQuery(user.GetCurrentUserName(), pageIndex, pageSize),
                    cancellationToken), StorageResponse.Create)));

        storagesApi.MapGet("/{id}", async (int id, ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher,
                CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new GetStorageQuery(id, user.GetCurrentUserName()), cancellationToken)
                is { } storage
                ? Results.Ok(StorageResponse.Create(storage))
                : Results.NotFound());

        storagesApi.MapPut("/{id}", async (int id, [FromBody] StorageForm form, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new UpdateStorageCommand(id, form, user.GetCurrentUserName()),
                    cancellationToken)
                is { } storage
                ? Results.Accepted($"/storages/{storage.Id}", StorageResponse.Create(storage))
                : Results.NotFound());

        storagesApi.MapDelete("/{id}", async (int id, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new DeleteStorageCommand(id, user.GetCurrentUserName()),
                    cancellationToken)
                ? Results.NoContent()
                : Results.NotFound());

        storagesApi.MapPost("/create", async ([FromBody] StorageForm form, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new CreateStorageCommand(form, user.GetCurrentUserName()),
                    cancellationToken)
                is { } storage
                ? Results.Created($"/storages/{storage.Id}", StorageResponse.Create(storage))
                : Results.NotFound());

        return app;
    }
}
