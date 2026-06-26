using Dobley.Domain.Core.Entities;
using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Entities.Storages;

namespace Dobley.Domain.Core.Forms;

public class ProductForm
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public Category? Category { get; set; }

    public decimal? Unit { get; set; }

    public UnitType? UnitType { get; set; }

    public string? Barcode { get; set; }

    public int? StorageId { get; set; }

    public Storage? DomainStorage { get; set; }

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
            StorageId = product.StorageId
        };

    public Product ToEntity()
        => Product.Create(Name!, Description!, Category!.Value, Unit!.Value, UnitType!.Value, Price!.Value, Barcode!,
            DomainStorage!);
}