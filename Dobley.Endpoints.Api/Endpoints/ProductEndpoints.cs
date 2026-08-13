using System.Security.Claims;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Products;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Dobley.Endpoints.Api.Endpoints;

public static class ProductEndpoints
{
    private const string StorageNotFoundMessage =
        "Хранилище не найдено или не принадлежит текущему пользователю.";

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var productsApi = app.MapGroup("/products").RequireAuthorization();

        productsApi.MapGet("/", async ([FromQuery] int? pageIndex, [FromQuery] int? pageSize, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => Results.Ok(PaginatedResponse<ProductResponse>.Create(
                await dispatcher.DispatchAsync(new GetProductsQuery(user.GetCurrentUserName(), pageIndex, pageSize),
                    cancellationToken), ProductResponse.Create)));

        productsApi.MapGet("/{id}", async (int id, ClaimsPrincipal user, [FromServices] IUseCaseDispatcher dispatcher,
                CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new GetProductQuery(id, user.GetCurrentUserName()), cancellationToken)
                is { } product
                ? Results.Ok(ProductResponse.Create(product))
                : Results.NotFound());

        productsApi.MapPut("/{id}", async (int id, [FromBody] ProductForm form, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new UpdateProductCommand(id, form, user.GetCurrentUserName()),
                    cancellationToken)
                is { } product
                ? Results.Accepted($"/products/{product.Id}", ProductResponse.Create(product))
                : Results.NotFound());

        productsApi.MapDelete("/{id}", async (int id, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new DeleteProductCommand(id, user.GetCurrentUserName()),
                    cancellationToken)
                ? Results.NoContent()
                : Results.NotFound());

        productsApi.MapPost("/create", async ([FromBody] ProductForm form, ClaimsPrincipal user,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken)
            => await dispatcher.DispatchAsync(new CreateProductCommand(form, user.GetCurrentUserName()),
                    cancellationToken)
                is { } product
                ? Results.Created($"/products/{product.Id}", ProductResponse.Create(product))
                : Results.BadRequest(new { error = StorageNotFoundMessage }));

        return app;
    }
}
