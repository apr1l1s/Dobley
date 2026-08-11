using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Dobley.Data.Core;
using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Storages;
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
const string PRODUCT_DICTIONARIES_CACHE_POLICY = "ProductDictionaries";

var productCategories = GetProductDictionary<Category>();
var productUnitTypes = GetProductDictionary<UnitType>();

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
    .AddOutputCache(options => options.AddPolicy(PRODUCT_DICTIONARIES_CACHE_POLICY,
        policy => policy.Expire(TimeSpan.FromHours(24))))
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
app.UseOutputCache();

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

var productDictionariesApi = app.MapGroup("/products");

productDictionariesApi.MapGet("/categories", (HttpContext httpContext) =>
    {
        SetProductDictionaryCacheHeaders(httpContext);

        return Results.Ok(productCategories);
    })
    .CacheOutput(PRODUCT_DICTIONARIES_CACHE_POLICY);

productDictionariesApi.MapGet("/unit-types", (HttpContext httpContext) =>
    {
        SetProductDictionaryCacheHeaders(httpContext);

        return Results.Ok(productUnitTypes);
    })
    .CacheOutput(PRODUCT_DICTIONARIES_CACHE_POLICY);

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

var notificationsApi = app.MapGroup("/notifications").RequireAuthorization();

notificationsApi.MapPost("/invites/create", async ([FromBody] CreateNotificationInviteRequest? request,
    ClaimsPrincipal user, [FromServices] INotificationInviteRepository notificationInviteRepository,
    [FromServices] ICommonRepository commonRepository, CancellationToken cancellationToken) =>
{
    var expiresAt = request?.ExpiresAt ?? DateTime.UtcNow.AddDays(1);
    var invite = NotificationInvite.Create(GetCurrentUserName(user), NotificationInviteCodeGenerator.Create(),
        expiresAt);

    await notificationInviteRepository.AddAsync(invite, cancellationToken);
    await commonRepository.SaveChangesAsync(cancellationToken);

    return Results.Created($"/notifications/invites/{invite.Id}", NotificationInviteResponse.Create(invite));
});

notificationsApi.MapGet("/recipients", async (ClaimsPrincipal user,
        [FromServices] INotificationRecipientRepository notificationRecipientRepository,
        CancellationToken cancellationToken)
    => Results.Ok((await notificationRecipientRepository.GetCollectionForUserAsync(GetCurrentUserName(user),
            cancellationToken))
        .Select(NotificationRecipientResponse.Create)));

notificationsApi.MapPost("/recipients/{recipientId}/subscriptions", async (int recipientId,
    [FromBody] StorageNotificationSubscriptionRequest? request, ClaimsPrincipal user,
    [FromServices] INotificationRecipientRepository notificationRecipientRepository,
    [FromServices] IStorageNotificationSubscriptionRepository storageNotificationSubscriptionRepository,
    [FromServices] IStorageRepository storageRepository, [FromServices] ICommonRepository commonRepository,
    CancellationToken cancellationToken) =>
{
    var userName = GetCurrentUserName(user);
    if (request?.StorageIds is not { Count: > 0 })
    {
        return Results.BadRequest(new { error = "Необходимо указать хотя бы одно хранилище" });
    }

    var recipient = await notificationRecipientRepository.GetForUserAsync(recipientId, userName, cancellationToken);
    if (recipient == null)
    {
        return Results.NotFound();
    }

    var storageIds = request.StorageIds.Distinct().ToArray();
    var ownedStorageIds = await storageRepository.GetOwnedStorageIdsAsync(userName, storageIds, cancellationToken);

    if (ownedStorageIds.Count != storageIds.Length)
    {
        return Results.BadRequest(new { error = "Одно или несколько хранилищ не найдены" });
    }

    var existingSubscriptions = await storageNotificationSubscriptionRepository.GetForRecipientAsync(recipientId,
        ownedStorageIds, cancellationToken);
    foreach (var subscription in existingSubscriptions.Where(x => !x.IsEnabled))
    {
        subscription.Enable();
    }

    var newSubscriptions = ownedStorageIds
        .Except(existingSubscriptions.Select(x => x.StorageId))
        .Select(storageId => StorageNotificationSubscription.Create(recipient.Id, storageId,
            request.NotifyBeforeDays))
        .ToArray();

    await storageNotificationSubscriptionRepository.AddRangeAsync(newSubscriptions, cancellationToken);
    await commonRepository.SaveChangesAsync(cancellationToken);

    return Results.Ok(existingSubscriptions
        .Concat(newSubscriptions)
        .Select(StorageNotificationSubscriptionResponse.Create));
});

notificationsApi.MapDelete("/recipients/{recipientId}/subscriptions", async (int recipientId, ClaimsPrincipal user,
    [FromServices] INotificationRecipientRepository notificationRecipientRepository,
    [FromServices] IStorageNotificationSubscriptionRepository storageNotificationSubscriptionRepository,
    [FromServices] ICommonRepository commonRepository, CancellationToken cancellationToken) =>
{
    var userName = GetCurrentUserName(user);
    var recipient = await notificationRecipientRepository.GetForUserAsync(recipientId, userName, cancellationToken);
    if (recipient == null)
    {
        return Results.NotFound();
    }

    var subscriptions = await storageNotificationSubscriptionRepository.GetForRecipientAsync(recipientId,
        cancellationToken);
    foreach (var subscription in subscriptions.Where(x => x.IsEnabled))
    {
        subscription.Disable();
    }

    await commonRepository.SaveChangesAsync(cancellationToken);

    return Results.NoContent();
});

notificationsApi.MapDelete("/recipients/{recipientId}", async (int recipientId, ClaimsPrincipal user,
    [FromServices] INotificationRecipientRepository notificationRecipientRepository,
    [FromServices] IStorageNotificationSubscriptionRepository storageNotificationSubscriptionRepository,
    [FromServices] ICommonRepository commonRepository, CancellationToken cancellationToken) =>
{
    var userName = GetCurrentUserName(user);
    var recipient = await notificationRecipientRepository.GetForUserAsync(recipientId, userName, cancellationToken);
    if (recipient == null)
    {
        return Results.NotFound();
    }

    var now = DateTime.UtcNow;
    var subscriptions = await storageNotificationSubscriptionRepository.GetForRecipientAsync(recipientId,
        cancellationToken);
    foreach (var subscription in subscriptions)
    {
        subscription.Delete(now);
    }

    recipient.Delete(now);
    await commonRepository.SaveChangesAsync(cancellationToken);

    return Results.NoContent();
});

app.Run();

static string GetCurrentUserName(ClaimsPrincipal user)
    => user.FindFirstValue(ClaimTypes.Name)
       ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
       ?? throw new UnauthorizedAccessException("User name claim is required.");

static IReadOnlyList<ProductDictionaryItemResponse> GetProductDictionary<TEnum>()
    where TEnum : struct, Enum
    => Enum.GetValues<TEnum>()
        .Select(value => new ProductDictionaryItemResponse(value.ToString(), GetDisplayName(value)))
        .ToArray();

static void SetProductDictionaryCacheHeaders(HttpContext httpContext)
{
    httpContext.Response.Headers.CacheControl = "public, max-age=86400";
}

static string GetDisplayName<TEnum>(TEnum value)
    where TEnum : struct, Enum
{
    var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();

    return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
}

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
