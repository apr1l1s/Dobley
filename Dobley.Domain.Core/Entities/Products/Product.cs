using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Products;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public Category Category { get; set; }

    public decimal Unit { get; set; }

    public UnitType UnitType { get; set; }

    public string Barcode { get; set; } = null!;

    public int StorageId { get; set; }

    public Storage? DomainStorage { get; set; }

    private Product()
    {
    }

    public static Product Create(string name, string description, Category category, decimal unit, UnitType unitType,
        decimal price, string barcode, Storage storage)
    {
        var product = Create(name, description, category.ToString(), unit, unitType.ToString(), price, barcode,
            storage.Id);
        product.DomainStorage = storage;

        return product;
    }

    public static Product Create(string name, string description, string category, decimal unit, string unitType,
        decimal price, string barcode, int storageId)
    {
        if (name.IsNullOrEmpty() || name.Length > 100)
        {
            throw new DomainValidateProductException("Название продукта должно быть не пустым и меньше 100 символов");
        }

        if (description.IsNullOrEmpty() || description.Length > 200)
        {
            throw new DomainValidateProductException("Описание продукта должно быть не пустым и меньше 200 символов");
        }

        if (category.IsNullOrEmpty() || !Enum.TryParse(category, true, out Category parsedCategory))
        {
            throw new DomainValidateProductException("Неизвестная категория");
        }

        if (unit < 0)
        {
            throw new DomainValidateProductException("Количество продукта не может быть отрицательным");
        }

        if (unitType.IsNullOrEmpty() || !Enum.TryParse(unitType, true, out UnitType parsedUnitType))
        {
            throw new DomainValidateProductException("Неизвестный тип измерения");
        }

        if (price < 0)
        {
            throw new DomainValidateProductException("Цена не может быть отрицательной");
        }

        if (barcode.IsNullOrEmpty())
        {
            throw new DomainValidateProductException("Неизвестный формат штрихкода");
        }

        if (storageId <= 0)
        {
            throw new DomainValidateProductException("Неизвестный идентификатор хранилища");
        }

        return new Product
        {
            Name = name,
            Description = description,
            Category = parsedCategory,
            Unit = unit,
            UnitType = parsedUnitType,
            Price = price,
            Barcode = barcode,
            StorageId = storageId
        };
    }

    public Product Update(string? name, string? description, Category? category, decimal? unit, UnitType? unitType,
        decimal? price, string? barcode)
    {
        var updatedProduct = Create(
            name ?? Name,
            description ?? Description,
            (category ?? Category).ToString(),
            unit ?? Unit,
            (unitType ?? UnitType).ToString(),
            price ?? Price,
            barcode ?? Barcode,
            StorageId
        );

        if (name != null)
        {
            Name = updatedProduct.Name;
        }

        if (description != null)
        {
            Description = updatedProduct.Description;
        }

        if (category != null)
        {
            Category = updatedProduct.Category;
        }

        if (unit != null)
        {
            Unit = updatedProduct.Unit;
        }

        if (unitType != null)
        {
            UnitType = updatedProduct.UnitType;
        }

        if (price != null)
        {
            Price = updatedProduct.Price;
        }

        if (barcode != null)
        {
            Barcode = updatedProduct.Barcode;
        }

        return this;
    }
}