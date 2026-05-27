namespace Dobley.Domain.Core.Entities.Products;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public string Category { get; set; }

    public decimal Unit { get; set; }

    public string UnitType { get; set; }

    public string Barcode { get; set; }

    private Product()
    {
    }

    public static Product Create(string name, string description, string category, decimal unit, string unitType,
        decimal price, string barcode)
    {
        return new Product()
        {
            Name = name,
            Description = description,
            Category = category,
            Unit = unit,
            UnitType = unitType,
            Price = price,
            Barcode = barcode
        };
    }

    public Product Update(string? name, string? description, string? category, decimal? unit, string? unitType,
        decimal? price, string? barcode)
    {
        if (name != null)
        {
            Name = name;
        }

        if (description != null)
        {
            Description = description;
        }

        if (category != null)
        {
            Category = category;
        }

        if (unit != null)
        {
            Unit = unit.Value;
        }

        if (unitType != null)
        {
            UnitType = unitType;
        }

        if (price != null)
        {
            Price = price.Value;
        }

        if (barcode != null)
        {
            Barcode = barcode;
        }

        return this;
    }
}