using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Domain.Core.Repositories.Products;

public interface IProductRepository : IRepository<Product, ProductFilter>
{
    Task<IReadOnlyList<Product>> GetExpiringProductsAsync(IReadOnlyCollection<int> storageIds, DateTime fromDate,
        DateTime toDate, CancellationToken cancellationToken = default);

    Task<Product?> GetOwnedProductAsync(int id, string userName, CancellationToken cancellationToken = default);
}
