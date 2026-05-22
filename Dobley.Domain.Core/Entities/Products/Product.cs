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
        decimal price)
    {
        return new Product()
        {
            Name = name,
            Description = description,
            Category = category,
            Unit = unit,
            UnitType = unitType,
            Price = price
        };
    }
}