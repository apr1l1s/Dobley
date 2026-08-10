using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Products;

public class ProductRepository(DobleyContext context) : IProductRepository
{
    public Task<Product?> GetItemNullable(int id) => FilterEntities(new ProductFilter(id)).FirstOrDefaultAsync();

    public Task<Product> GetItem(int id) => FilterEntities(new ProductFilter(id)).FirstOrDefaultAsync()!;

    public async Task<IReadOnlyList<Product>> GetCollectionAsync(params int[] ids)
        => await context.Products.Where(x => ids.Contains(x.Id)).ToListAsync();

    public async Task<IReadOnlyList<Product>?> GetCollectionAsync(ProductFilter filter)
        => await FilterEntities(filter).ToListAsync();

    public Task<PaginatedCollection<Product>> GetPaginatedCollection(ProductFilter? filter = null, int pageIndex = 1,
        int pageSize = 10)
        => ToPaginatedCollection(FilterEntities(filter), pageIndex, pageSize);

    public async Task<Product> AddAsync(Product product) => (await context.AddAsync(product)).Entity;

    private IQueryable<Product> FilterEntities(ProductFilter? filter)
    {
        var products = context.Products.AsQueryable();

        if (filter == null)
        {
            return products;
        }

        if (filter.Ids != null)
        {
            products = products.Where(x => filter.Ids.Contains(x.Id));
        }

        if (filter.Names != null)
        {
            products = products.Where(x => filter.Names.Contains(x.Name));
        }

        return products;
    }

    public async Task<PaginatedCollection<TEntity>> ToPaginatedCollection<TEntity>(IQueryable<TEntity> query,
        int pageIndex, int pageSize)
    {
        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        if (pageSize is < 1 or > 100)
        {
            pageSize = 10;
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageIndex - 1) * pageSize) // Пропустить элементы предыдущих страниц
            .Take(pageSize) // Взять элементы текущей страницы
            .ToListAsync();

        return new PaginatedCollection<TEntity>(items, pageIndex, pageSize, totalCount);
    }
}
