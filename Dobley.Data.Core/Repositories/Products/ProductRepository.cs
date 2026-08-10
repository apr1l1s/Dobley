using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Products;

public class ProductRepository(DobleyContext context)
    : RepositoryBase<Product, ProductFilter>(context), IProductRepository
{
    public override async Task<IReadOnlyList<Product>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new ProductFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<Product>?> GetCollectionAsync(ProductFilter filter,
        CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public override Task<PaginatedCollection<Product>> GetPaginatedCollection(ProductFilter? filter = null,
        int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageIndex, pageSize, cancellationToken);

    public Task<Product?> GetOwnedProductAsync(int id, string userName, CancellationToken cancellationToken = default)
        => FilterEntities(new ProductFilter(id).SetUserNames([userName])).FirstOrDefaultAsync(cancellationToken);

    private IQueryable<Product> FilterEntities(ProductFilter? filter)
    {
        var products = Context.Products.Include(x => x.DomainStorage).AsQueryable();

        if (filter == null)
        {
            return products;
        }

        if (filter.Ids is { Count: > 0 })
        {
            products = products.Where(x => filter.Ids.Contains(x.Id));
        }

        if (filter.Names is { Count: > 0 })
        {
            products = products.Where(x => filter.Names.Contains(x.Name));
        }

        if (filter.UserNames is { Count: > 0 })
        {
            products = products.Where(x => filter.UserNames.Contains(x.DomainStorage!.UserName));
        }

        return products;
    }
}
