using System.Security.Claims;
using System.Text;
using Dobley.Data.Core;
using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Products;
using Dobley.Domain.Core.UseCases.Storages;
using Dobley.Endpoints.Api.ExceptionHandling;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddDobleyLogging("Dobley.Endpoints.Api");

const string JWT_AUTHENTICATION_FAILED_MESSAGE =
    "Ошибка JWT-аутентификации.";
const string STORAGE_NOT_FOUND_MESSAGE =
    "Хранилище не найдено или не принадлежит текущему пользователю.";

var isLocal = builder.Configuration.GetValue<bool>("ASPNET_LOCAL");

builder.Host.ConfigureAppServices(isLocal);

var services = builder.Services;
services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = DependencyInjection.GetJwtIssuer(),
            ValidAudience = DependencyInjection.GetJwtAudience(),
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DependencyInjection.GetRequiredSecretKey()))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogWarning(context.Exception, JWT_AUTHENTICATION_FAILED_MESSAGE);

                return Task.CompletedTask;
            }
        };
    });

AddApiExceptionHandling(builder.Services);

builder.Services
    .AddAuthorization()
    .ConfigureHttpJsonOptions(options => DependencyInjection.AddDefaultJsonConverters(options.SerializerOptions))
    .Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true)
    .AddCoreServices()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();
await app.Services.SeedDevelopmentDataAsync();

app.UseDobleyRequestLogging();
app.UseExceptionHandler();

if (isLocal)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/admin", () => Results.Ok()).RequireAuthorization();
app.MapGet("/health", async ([FromServices] DobleyContext db, [FromServices] IDistributedCache cache,
    CancellationToken cancellationToken) =>
{
    var databaseAvailable = await db.Database.CanConnectAsync(cancellationToken);
    var cacheAvailable = await CheckCache(cache, cancellationToken);

    return databaseAvailable && cacheAvailable
        ? Results.Ok(new HealthResponse("Healthy", databaseAvailable, cacheAvailable))
        : Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
});

var productsApi = app.MapGroup("/products").RequireAuthorization();

productsApi.MapGet("/", async ([FromQuery] int? pageIndex, [FromQuery] int? pageSize, ClaimsPrincipal user,
        [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
    => Results.Ok(PaginatedResponse<ProductResponse>.Create(
        await dispatcher.DispatchAsync(new GetProductsUseCase(GetCurrentUserName(user), pageIndex, pageSize),
            cancellationToken), ProductResponse.Create)));

productsApi.MapGet("/{id}", async (int id, ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher,
        CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new GetProductUseCase(id, GetCurrentUserName(user)), cancellationToken)
        is { } product
        ? Results.Ok(ProductResponse.Create(product))
        : Results.NotFound());

productsApi.MapPut("/{id}", async (int id, [FromBody] ProductForm form, ClaimsPrincipal user,
        [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new PutProductUseCase(id, form, GetCurrentUserName(user)), cancellationToken)
        is { } product
        ? Results.Accepted($"/products/{product.Id}", ProductResponse.Create(product))
        : Results.NotFound());

productsApi.MapDelete("/{id}", async (int id, ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher,
        CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new DeleteProductUseCase(id, GetCurrentUserName(user)), cancellationToken)
        ? Results.NoContent()
        : Results.NotFound());

productsApi.MapPost("/create", async ([FromBody] ProductForm form, ClaimsPrincipal user,
        [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new CreateProductUseCase(form, GetCurrentUserName(user)), cancellationToken)
        is { } product
        ? Results.Created($"/products/{product.Id}", ProductResponse.Create(product))
        : Results.BadRequest(new { error = STORAGE_NOT_FOUND_MESSAGE }));

var storagesApi = app.MapGroup("/storages").RequireAuthorization();

storagesApi.MapGet("/", async ([FromQuery] int? pageIndex, [FromQuery] int? pageSize, ClaimsPrincipal user,
        [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
    => Results.Ok(PaginatedResponse<StorageResponse>.Create(
        await dispatcher.DispatchAsync(new GetStoragesUseCase(GetCurrentUserName(user), pageIndex, pageSize),
            cancellationToken), StorageResponse.Create)));

storagesApi.MapGet("/{id}", async (int id, ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher,
        CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new GetStorageUseCase(id, GetCurrentUserName(user)), cancellationToken)
        is { } storage
        ? Results.Ok(StorageResponse.Create(storage))
        : Results.NotFound());

storagesApi.MapPut("/{id}", async (int id, [FromBody] StorageForm form, ClaimsPrincipal user,
        [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new PutStorageUseCase(id, form, GetCurrentUserName(user)), cancellationToken)
        is { } storage
        ? Results.Accepted($"/storages/{storage.Id}", StorageResponse.Create(storage))
        : Results.NotFound());

storagesApi.MapDelete("/{id}", async (int id, ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher,
        CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new DeleteStorageUseCase(id, GetCurrentUserName(user)), cancellationToken)
        ? Results.NoContent()
        : Results.NotFound());

storagesApi.MapPost("/create", async ([FromBody] StorageForm form, ClaimsPrincipal user,
        [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
    => await dispatcher.DispatchAsync(new CreateStorageUseCase(form, GetCurrentUserName(user)), cancellationToken)
        is { } storage
        ? Results.Created($"/storages/{storage.Id}", StorageResponse.Create(storage))
        : Results.NotFound());

app.Run();

static string GetCurrentUserName(ClaimsPrincipal user)
    => user.FindFirstValue(ClaimTypes.Name)
       ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
       ?? throw new UnauthorizedAccessException("User name claim is required.");

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

static void AddApiExceptionHandling(IServiceCollection services)
{
    services.AddExceptionHandler<ApiExceptionHandler>();
    services.AddProblemDetails();
}
