using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Products;

public class Product
    : IAuditableEntity, ISoftDeletedEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public Category Category { get; set; }

    public decimal Unit { get; set; }

    public UnitType UnitType { get; set; }

    public string Barcode { get; set; } = null!;

    public DateTime? ExpirationDate { get; private set; }

    public int StorageId { get; set; }

    public Storage? DomainStorage { get; set; }

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    public DateTime? DateDeleted { get; private set; }

    public bool IsDeleted => DateDeleted.HasValue;

    private Product()
    {
    }

    public static Product Create(string name, string description, Category category, decimal unit, UnitType unitType,
        decimal price, string barcode, Storage storage, DateTime? expirationDate = null)
    {
        var product = Create(name, description, category.ToString(), unit, unitType.ToString(), price, barcode,
            storage.Id, expirationDate);
        product.DomainStorage = storage;

        return product;
    }

    public static Product Create(string name, string description, string category, decimal unit, string unitType,
        decimal price, string barcode, int storageId, DateTime? expirationDate = null)
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
            ExpirationDate = expirationDate,
            StorageId = storageId
        };
    }

    public Product Update(string? name, string? description, Category? category, decimal? unit, UnitType? unitType,
        decimal? price, string? barcode, DateTime? expirationDate = null)
    {
        var updatedProduct = Create(
            name ?? Name,
            description ?? Description,
            (category ?? Category).ToString(),
            unit ?? Unit,
            (unitType ?? UnitType).ToString(),
            price ?? Price,
            barcode ?? Barcode,
            StorageId,
            expirationDate ?? ExpirationDate
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

        if (expirationDate != null)
        {
            ExpirationDate = updatedProduct.ExpirationDate;
        }

        return this;
    }

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;

    public void Delete(DateTime dateDeleted) => DateDeleted = dateDeleted;
}
