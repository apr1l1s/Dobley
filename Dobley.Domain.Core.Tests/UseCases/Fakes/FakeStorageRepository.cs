using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Storages;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public class FakeStorageRepository(params Storage[] storages) : IStorageRepository
{
    private readonly List<Storage> _storages = [..storages];

    public IReadOnlyList<Storage> AddedStorages => _storages;

    public IReadOnlyList<Storage> DeletedStorages => _deletedStorages;

    private readonly List<Storage> _deletedStorages = [];

    public Task<Storage> AddAsync(Storage entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id == 0)
        {
            entity.Id = _storages.Count == 0 ? 1 : _storages.Max(x => x.Id) + 1;
        }

        _storages.Add(entity);
        return Task.FromResult(entity);
    }

    public void Delete(Storage entity)
    {
        _deletedStorages.Add(entity);
    }

    public Task<IReadOnlyList<Storage>> GetCollectionAsync(CancellationToken cancellationToken = default,
        params int[] ids)
        => Task.FromResult<IReadOnlyList<Storage>>(_storages.Where(x => ids.Contains(x.Id)).ToArray());

    public Task<IReadOnlyList<Storage>?> GetCollectionAsync(StorageFilter filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Storage>?>(ApplyFilter(filter).ToArray());

    public Task<Storage> GetItem(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_storages.Single(x => x.Id == id));

    public Task<Storage?> GetItemNullable(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_storages.SingleOrDefault(x => x.Id == id));

    public Task<Storage?> GetOwnedStorageAsync(int id, string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(_storages.SingleOrDefault(x => x.Id == id && x.UserName == userName));

    public Task<IReadOnlyList<int>> GetOwnedStorageIdsAsync(string userName, IReadOnlyCollection<int> storageIds,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<int>>(_storages
            .Where(x => x.UserName == userName && storageIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToArray());

    public Task<IReadOnlyList<int>> GetStorageIdsAsync(string userName,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<int>>(_storages
            .Where(x => x.UserName == userName)
            .Select(x => x.Id)
            .ToArray());

    public Task<PaginatedCollection<Storage>> GetPaginatedCollection(StorageFilter? filter, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var collection = ApplyFilter(filter).ToArray();
        return Task.FromResult(new PaginatedCollection<Storage>(collection, pageNumber, pageSize, collection.Length));
    }

    private IEnumerable<Storage> ApplyFilter(StorageFilter? filter)
    {
        var storages = _storages.AsEnumerable();
        if (filter?.Ids is { Count: > 0 })
        {
            storages = storages.Where(x => filter.Ids.Contains(x.Id));
        }

        if (filter?.UserNames is { Count: > 0 })
        {
            storages = storages.Where(x => filter.UserNames.Contains(x.UserName));
        }

        return storages;
    }
}
