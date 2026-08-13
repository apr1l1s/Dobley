using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeProductRepository(params Product[] products) : IProductRepository
{
    private readonly List<Product> _products = [..products];

    public IReadOnlyList<Product> AddedProducts => _products;

    public IReadOnlyList<Product> DeletedProducts => _deletedProducts;

    private readonly List<Product> _deletedProducts = [];

    public Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id == 0)
        {
            entity.Id = _products.Count == 0 ? 1 : _products.Max(x => x.Id) + 1;
        }

        _products.Add(entity);
        return Task.FromResult(entity);
    }

    public void Delete(Product entity)
    {
        _deletedProducts.Add(entity);
    }

    public Task<IReadOnlyList<Product>> GetCollectionAsync(CancellationToken cancellationToken = default,
        params int[] ids)
        => Task.FromResult<IReadOnlyList<Product>>(_products.Where(x => ids.Contains(x.Id)).ToArray());

    public Task<IReadOnlyList<Product>?> GetCollectionAsync(ProductFilter filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>?>(ApplyFilter(filter).ToArray());

    public Task<IReadOnlyList<Product>> GetExpiringProductsAsync(IReadOnlyCollection<int> storageIds,
        DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>>(_products
            .Where(x => storageIds.Contains(x.StorageId) &&
                        x.ExpirationDate != null &&
                        x.ExpirationDate.Value.Date >= fromDate.Date &&
                        x.ExpirationDate.Value.Date < toDate.Date)
            .ToArray());

    public Task<Product> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Single(x => x.Id == id));

    public Task<Product?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.SingleOrDefault(x => x.Id == id));

    public Task<Product?> GetOwnedProductAsync(int id, string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.SingleOrDefault(x => x.Id == id && x.DomainStorage?.UserName == userName));

    public Task<IReadOnlyList<Product>> GetStorageProductsAsync(int storageId, string userName,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>>(_products
            .Where(x => x.StorageId == storageId && x.DomainStorage?.UserName == userName)
            .ToArray());

    public Task<PaginatedCollection<Product>> GetPaginatedCollection(ProductFilter? filter, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var collection = ApplyFilter(filter).ToArray();
        return Task.FromResult(new PaginatedCollection<Product>(collection, pageNumber, pageSize, collection.Length));
    }

    private IEnumerable<Product> ApplyFilter(ProductFilter? filter)
    {
        var products = _products.AsEnumerable();
        if (filter?.Ids is { Count: > 0 })
        {
            products = products.Where(x => filter.Ids.Contains(x.Id));
        }

        if (filter?.StorageIds is { Count: > 0 })
        {
            products = products.Where(x => filter.StorageIds.Contains(x.StorageId));
        }

        if (filter?.UserNames is { Count: > 0 })
        {
            products = products.Where(x => x.DomainStorage != null && filter.UserNames.Contains(x.DomainStorage.UserName));
        }

        return products;
    }
}
