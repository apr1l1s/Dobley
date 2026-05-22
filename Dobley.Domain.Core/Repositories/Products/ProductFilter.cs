namespace Dobley.Domain.Core.Repositories.Products;

public class ProductFilter(params int[] ids)
{
    public IReadOnlyList<string>? Names { get; set; }

    public IReadOnlyList<int>? Ids { get; set; } = ids;

    public ProductFilter SetNames(IReadOnlyList<string> names)
    {
        Names = names;

        return this;
    }

    public ProductFilter SetIds(IReadOnlyList<int> ids)
    {
        Ids = ids;

        return this;
    }
}