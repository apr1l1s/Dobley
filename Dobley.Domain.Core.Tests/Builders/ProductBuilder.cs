using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Domain.Core.Tests.Builders;

public static class ProductBuilder
{
    public static int LastId { get; private set; }

    public static Product Build(int? id = null, string name = "Milk", string description = "Fresh milk",
        string? category = null, decimal unit = 1, string? unitType = null, decimal price = 120,
        string barcode = "4600000000000", int storageId = 1)
    {
        var product = Product.Create(name, description, category ?? Category.Dairy.ToString(), unit,
            unitType ?? UnitType.Liters.ToString(), price, barcode, storageId);

        product.Id = id ?? ++LastId;

        return product;
    }
}
