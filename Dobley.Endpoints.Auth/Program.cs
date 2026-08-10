using Dobley.Data.Core;
using Dobley.Data.Core.Context;
using Dobley.Endpoints.Auth.Dto;
using Dobley.Domain.Core.Repositories.Users;
using Dobley.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddDobleyLogging("Dobley.Endpoints.Auth");

var isLocal = builder.Configuration.GetValue<bool>("ASPNET_LOCAL");

builder.Host.ConfigureAppServices(isLocal);

var services = builder.Services;
services.AddAuthServices();
services.ConfigureHttpJsonOptions(options => DependencyInjection.AddDefaultJsonConverters(options.SerializerOptions));
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();
app.UseDobleyRequestLogging();

if (isLocal)
{
    await app.Services.SeedDevelopmentDataAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", async ([FromServices] DobleyContext db, [FromServices] IDistributedCache cache,
    CancellationToken cancellationToken) =>
{
    var databaseAvailable = await db.Database.CanConnectAsync(cancellationToken);
    var cacheAvailable = await CheckCache(cache, cancellationToken);

    return databaseAvailable && cacheAvailable
        ? Results.Ok(new HealthResponse("Healthy", databaseAvailable, cacheAvailable))
        : Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapPost("/login", async ([FromBody] UserCredentials credentials, [FromServices] IAuthService auth,
        CancellationToken cancellationToken)
    => await auth.Login(credentials.Login, credentials.Password, cancellationToken) is { } tokens
        ? Results.Ok(AuthTokens.Create(tokens))
        : Results.Unauthorized());

app.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, [FromServices] IAuthService auth,
        CancellationToken cancellationToken)
    => await auth.Refresh(request.RefreshToken, cancellationToken) is { } tokens
        ? Results.Ok(AuthTokens.Create(tokens))
        : Results.Unauthorized());

app.MapPost("/logout", async ([FromBody] RefreshTokenRequest request, [FromServices] IAuthService auth,
    CancellationToken cancellationToken) =>
{
    await auth.Logout(request.RefreshToken, cancellationToken);
    return Results.NoContent();
});

app.MapPost("/reg", async ([FromBody] UserCredentials credentials, [FromServices] IAuthService auth,
        CancellationToken cancellationToken)
        => await auth.Register(credentials.Login, credentials.Password, cancellationToken)
            ? Results.Ok()
            : Results.Conflict())
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status409Conflict);

app.Run();

static async Task<bool> CheckCache(IDistributedCache cache, CancellationToken cancellationToken)
{
    try
    {
        var cacheKey = $"health:{Guid.NewGuid()}";
        await cache.SetStringAsync(cacheKey, "ok", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
        }, cancellationToken);

        var cacheAvailable = await cache.GetStringAsync(cacheKey, cancellationToken) == "ok";
        await cache.RemoveAsync(cacheKey, cancellationToken);

        return cacheAvailable;
    }
    catch
    {
        return false;
    }
}
