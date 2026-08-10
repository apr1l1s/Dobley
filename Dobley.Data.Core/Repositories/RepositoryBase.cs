using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories;

public abstract class RepositoryBase<TEntity, TFilter>(DobleyContext context)
    : IRepository<TEntity, TFilter>
    where TEntity : class
{
    protected DobleyContext Context { get; } = context;

    public async Task<TEntity> GetItem(int id, CancellationToken cancellationToken = default)
        => await GetItemNullable(id, cancellationToken) ??
           throw new InvalidOperationException($"{typeof(TEntity).Name} not found.");

    public async Task<TEntity?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => await Context.FindAsync<TEntity>([id], cancellationToken);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => (await Context.AddAsync(entity, cancellationToken)).Entity;

    public void Delete(TEntity entity) => Context.Remove(entity);

    public abstract Task<IReadOnlyList<TEntity>> GetCollectionAsync(CancellationToken cancellationToken = default,
        params int[] ids);

    public abstract Task<IReadOnlyList<TEntity>?> GetCollectionAsync(TFilter filter,
        CancellationToken cancellationToken = default);

    public abstract Task<PaginatedCollection<TEntity>> GetPaginatedCollection(TFilter? filter, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default);

    protected static async Task<PaginatedCollection<TCollectionEntity>> ToPaginatedCollection<TCollectionEntity>(
        IQueryable<TCollectionEntity> query, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default)
        where TCollectionEntity : class
    {
        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        if (pageSize is < 1 or > 100)
        {
            pageSize = 10;
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(entity => EF.Property<int>(entity, "Id"))
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedCollection<TCollectionEntity>(items, pageIndex, pageSize, totalCount);
    }
}
