namespace Dobley.Domain.Core.Repositories;

public interface IRepository<TEntity, in TFilter>
{
    Task<TEntity> GetItem(int id);

    Task<TEntity?> GetItemNullable(int id);

    Task<IReadOnlyList<TEntity>> GetCollectionAsync(params int[] ids);

    Task<IReadOnlyList<TEntity>?> GetCollectionAsync(TFilter filter);

    Task<PaginatedCollection<TEntity>> GetPaginatedCollection(TFilter? filter, int pageNumber = 1, int pageSize = 10);
}