using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Domain.Core.Forms;

public class ProductForm
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Category { get; set; }

    public decimal? Unit { get; set; }

    public string? UnitType { get; set; }

    public string? Barcode { get; set; }

    public static ProductForm ToForm(Product product)
        => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            Unit = product.Unit,
            UnitType = product.UnitType,
            Barcode = product.Barcode,
        };

    public Product? ToEntity()
    {
        if (Name.IsNullOrEmpty() ||
            Description.IsNullOrEmpty() ||
            Category.IsNullOrEmpty() ||
            Unit == null ||
            UnitType.IsNullOrEmpty() ||
            Price == null ||
            Barcode.IsNullOrEmpty())
        {
            return null;
        }

        return Product.Create(Name!, Description!, Category!, Unit.Value, UnitType!, Price.Value, Barcode);
    }
}