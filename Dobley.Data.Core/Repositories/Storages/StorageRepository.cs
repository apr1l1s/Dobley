using Dobley.Data.Core.Context;
using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Repositories.Storages;

public class StorageRepository(DobleyContext context)
    : RepositoryBase<Storage, StorageFilter>(context), IStorageRepository
{
    public override async Task<IReadOnlyList<Storage>> GetCollectionAsync(
        CancellationToken cancellationToken = default, params int[] ids)
        => await FilterEntities(new StorageFilter(ids)).ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<Storage>?> GetCollectionAsync(StorageFilter filter,
        CancellationToken cancellationToken = default)
        => await FilterEntities(filter).ToListAsync(cancellationToken);

    public override Task<PaginatedCollection<Storage>> GetPaginatedCollection(StorageFilter? filter, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
        => ToPaginatedCollection(FilterEntities(filter), pageNumber, pageSize, cancellationToken);

    public Task<Storage?> GetOwnedStorageAsync(int id, string userName, CancellationToken cancellationToken = default)
        => FilterEntities(new StorageFilter(id).SetUserNames([userName])).FirstOrDefaultAsync(cancellationToken);

    private IQueryable<Storage> FilterEntities(StorageFilter? filter)
    {
        var storages = Context.Storages.AsQueryable();

        if (filter == null)
        {
            return storages;
        }

        if (filter.Ids is { Count: > 0 })
        {
            storages = storages.Where(x => filter.Ids.Contains(x.Id));
        }

        if (filter.UserNames is { Count: > 0 })
        {
            storages = storages.Where(x => filter.UserNames.Contains(x.UserName));
        }

        return storages;
    }
}
