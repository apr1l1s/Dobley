using System.Text;
using Dobley.Data.Core;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);

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
            ValidIssuer = "your-auth-service",
            ValidAudience = "your-audience",
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY")!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCoreServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok());
app.MapGet("/admin", () => Results.Ok()).RequireAuthorization();

var productsApi = app.MapGroup("/products").RequireAuthorization();
productsApi.MapGet("/", async ([FromQuery] int? pageIndex, [FromQuery] int? pageSize,
        [FromServices] IUseCaseDispatcher dispatcher)
    => await dispatcher.DispatchAsync(new GetProductsUseCase(pageIndex, pageSize)) is { } collection
        ? Results.Ok(collection)
        : Results.NotFound());

productsApi.MapGet("/{id}", async (int id, [FromServices] IProductRepository products)
    => await products.GetItemNullable(id) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

productsApi.MapPut("/{id}", async (int id, [FromBody] ProductForm form, [FromServices] IUseCaseDispatcher dispatcher)
    => await dispatcher.DispatchAsync(new PutProductUseCase(id, form)) is { } product
        ? Results.Accepted($"/products/{product.Id}", product)
        : Results.NotFound());

productsApi.MapPost("/create", async ([FromBody] ProductForm form, [FromServices] IUseCaseDispatcher dispatcher)
    => await dispatcher.DispatchAsync(new CreateProductUseCase(form)) is { } product
        ? Results.Created($"/products/{product.Id}", product)
        : Results.NotFound());

app.Run();