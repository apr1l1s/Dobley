using System.Text;
using Dobley.Data.Core.Repositories;
using Dobley.Data.Core.Services;
using Dobley.Domain.Core.Repositories.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);
var services = builder.Services;
// services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
// });

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
            ValidIssuer = "your-auth-service",
            ValidAudience = "your-audience",
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY")!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDataBase()
    .AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok());
app.MapGet("/test", () => Results.Ok()).RequireAuthorization();

var productsApi = app.MapGroup("/products");
productsApi.MapGet("/", async ([FromServices] IProductRepository products, [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        => await products.GetPaginatedCollection(new ProductFilter(), pageIndex, pageSize) is { } collection
            ? Results.Ok(collection)
            : Results.NotFound())
    .RequireAuthorization();

productsApi.MapGet("/{id}", async (int id, [FromServices] IProductRepository products)
        => await products.GetItemNullable(id) is { } product
            ? Results.Ok(product)
            : Results.NotFound())
    .RequireAuthorization();

app.Run();