namespace Dobley.Domain.Core.Repositories;

public interface IRepository<TEntity, in TFilter>
{
    Task<TEntity> GetItem(int id, CancellationToken cancellationToken = default);

    Task<TEntity?> GetItemNullable(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetCollectionAsync(CancellationToken cancellationToken = default, params int[] ids);

    Task<IReadOnlyList<TEntity>?> GetCollectionAsync(TFilter filter, CancellationToken cancellationToken = default);

    Task<PaginatedCollection<TEntity>> GetPaginatedCollection(TFilter? filter, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Delete(TEntity entity);
}
