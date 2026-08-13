using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Health;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Dobley.Endpoints.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin", () => Results.Ok()).RequireAuthorization();

        app.MapGet("/health", async ([FromServices] IUseCaseDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var health = await dispatcher.DispatchAsync(new GetHealthQuery(), cancellationToken);

            return health.DatabaseAvailable && health.CacheAvailable
                ? Results.Ok(new HealthResponse(health.Status, health.DatabaseAvailable, health.CacheAvailable))
                : Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        return app;
    }
}
