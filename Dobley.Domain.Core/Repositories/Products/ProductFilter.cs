namespace Dobley.Domain.Core.Repositories.Products;

public class ProductFilter(params int[] ids)
{
    public IReadOnlyList<string>? Names { get; set; }

    public IReadOnlyList<int>? Ids { get; set; } = ids;

    public IReadOnlyList<int>? StorageIds { get; set; }

    public IReadOnlyList<string>? UserNames { get; set; }

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

    public ProductFilter SetStorageIds(IReadOnlyList<int> storageIds)
    {
        StorageIds = storageIds;

        return this;
    }

    public ProductFilter SetUserNames(IReadOnlyList<string> userNames)
    {
        UserNames = userNames;

        return this;
    }
}
