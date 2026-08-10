using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Endpoints.Api.Dto;

public record ProductResponse(int Id, string Name, string Description, decimal Price, string Category, decimal Unit,
    string UnitType, string Barcode, int StorageId)
{
    public static ProductResponse Create(Product product)
        => new(product.Id, product.Name, product.Description, product.Price, product.Category.ToString(), product.Unit,
            product.UnitType.ToString(), product.Barcode, product.StorageId);
}
