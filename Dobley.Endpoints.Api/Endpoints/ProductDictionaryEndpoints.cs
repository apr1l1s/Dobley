using Dobley.Domain.Core.UseCases;
using Dobley.Domain.Core.UseCases.Products;
using Dobley.Endpoints.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Dobley.Endpoints.Api.Endpoints;

public static class ProductDictionaryEndpoints
{
    public const string CACHE_POLICY = "ProductDictionaries";

    public static IEndpointRouteBuilder MapProductDictionaryEndpoints(this IEndpointRouteBuilder app)
    {
        var productDictionariesApi = app.MapGroup("/products");

        productDictionariesApi.MapGet("/categories", async (HttpContext httpContext,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                SetCacheHeaders(httpContext);
                var categories = await dispatcher.DispatchAsync(
                    new GetProductDictionaryQuery(ProductDictionaryKind.Categories), cancellationToken);

                return Results.Ok(categories.Select(x => new ProductDictionaryItemResponse(x.Name, x.DisplayName)));
            })
            .CacheOutput(CACHE_POLICY);

        productDictionariesApi.MapGet("/unit-types", async (HttpContext httpContext,
                [FromServices] IUseCaseDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                SetCacheHeaders(httpContext);
                var unitTypes = await dispatcher.DispatchAsync(
                    new GetProductDictionaryQuery(ProductDictionaryKind.UnitTypes), cancellationToken);

                return Results.Ok(unitTypes.Select(x => new ProductDictionaryItemResponse(x.Name, x.DisplayName)));
            })
            .CacheOutput(CACHE_POLICY);

        return app;
    }

    private static void SetCacheHeaders(HttpContext httpContext)
    {
        httpContext.Response.Headers.CacheControl = "public, max-age=86400";
    }
}
