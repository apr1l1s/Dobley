using System.Text;
using Dobley.Data.Core;
using Dobley.Data.Core.Context;
using Dobley.Endpoints.Api.Endpoints;
using Dobley.Endpoints.Api.ExceptionHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddDobleyLogging("Dobley.Endpoints.Api");

var isLocal = builder.Configuration.GetValue<bool>("ASPNET_LOCAL");

builder.Host.ConfigureAppServices(isLocal);

builder.Services.AddAuthentication(options =>
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
                logger.LogWarning(context.Exception, "Ошибка JWT-аутентификации.");

                return Task.CompletedTask;
            }
        };
    });

builder.Services
    .AddAuthorization()
    .AddExceptionHandler<ApiExceptionHandler>()
    .AddProblemDetails()
    .AddOutputCache(options => options.AddPolicy(ProductDictionaryEndpoints.CACHE_POLICY,
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

app
    .MapHealthEndpoints()
    .MapProductDictionaryEndpoints()
    .MapProductEndpoints()
    .MapStorageEndpoints()
    .MapNotificationEndpoints();

app.Run();