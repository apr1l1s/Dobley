using Dobley.Data.Core;
using Dobley.Data.Core.Context;
using Dobley.Endpoints.Api.Endpoints;
using Dobley.Endpoints.Api.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddDobleyLogging("Dobley.Endpoints.Api");

var isLocal = builder.Configuration.GetValue<bool>("ASPNET_LOCAL");
var isSwaggerEnabled = builder.Configuration.IsApiSwaggerEnabled();

builder.Host.ConfigureAppServices(isLocal);

builder.Services
    .AddApiAuthentication()
    .AddAuthorization()
    .AddExceptionHandler<ApiExceptionHandler>()
    .AddProblemDetails()
    .AddOutputCache(options => options.AddPolicy(ProductDictionaryEndpoints.CACHE_POLICY,
        policy => policy.Expire(TimeSpan.FromHours(24))))
    .ConfigureHttpJsonOptions(options => DependencyInjection.AddDefaultJsonConverters(options.SerializerOptions))
    .Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true)
    .AddCoreServices()
    .AddApiSwagger();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();
await app.Services.SeedDevelopmentDataAsync();

app.UseDobleyRequestLogging();
app.UseExceptionHandler();
app.UseOutputCache();

if (isSwaggerEnabled)
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
    .MapAdminDatabaseEndpoints();

app.Run();
