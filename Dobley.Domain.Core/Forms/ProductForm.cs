using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Errors.Entities;

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

    public DateTime? ExpirationDate { get; set; }

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
            ExpirationDate = product.ExpirationDate,
            StorageId = product.StorageId
        };

    public Product ToEntity()
    {
        var missedFieldNames = GetMissedFieldNames().ToArray();
        if (missedFieldNames.Length > 0)
        {
            throw new DomainValidateProductException(
                $"Не заполнены обязательные поля продукта: {string.Join(", ", missedFieldNames)}");
        }

        var name = Name!;
        var description = Description!;
        var category = Category!.Value;
        var unit = Unit!.Value;
        var unitType = UnitType!.Value;
        var price = Price!.Value;
        var barcode = Barcode!;
        var storageId = StorageId!.Value;

        return Product.Create(name, description, category.ToString(), unit, unitType.ToString(), price, barcode,
            storageId, ExpirationDate);
    }

    private IEnumerable<string> GetMissedFieldNames()
    {
        if (Name.IsNullOrEmpty())
        {
            yield return nameof(Name);
        }

        if (Description.IsNullOrEmpty())
        {
            yield return nameof(Description);
        }

        if (Category is null)
        {
            yield return nameof(Category);
        }

        if (Unit is null)
        {
            yield return nameof(Unit);
        }

        if (UnitType is null)
        {
            yield return nameof(UnitType);
        }

        if (Price is null)
        {
            yield return nameof(Price);
        }

        if (Barcode.IsNullOrEmpty())
        {
            yield return nameof(Barcode);
        }

        if (StorageId is null)
        {
            yield return nameof(StorageId);
        }
    }
}
